using System.Collections;
using System.Collections.Generic;
using API;
using ArtefactSystem;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Tools
{
    public class LabelManager : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Brush brush;

        [SerializeField] private Artefact artefact;

        [Header("State")] public List<Label> allLabels = new();
        public Label activeLabel;

        /// <summary>
        /// Fired once LoadSessionAsync finishes (success or failure).
        /// BrushUI subscribes to this to know when it is safe to build the label list UI.
        /// </summary>
        public UnityEvent OnLabelsLoaded = new();

        // Queue that holds the free label indices in the shader attributes
        private readonly Queue<int> _freeIndices = new();

        private static readonly int RedBandID = Shader.PropertyToID("_RedBand");
        private static readonly int GreenBandID = Shader.PropertyToID("_GreenBand");
        private static readonly int BlueBandID = Shader.PropertyToID("_BlueBand");

        private void Awake()
        {
            StartCoroutine(LoadSessionAsync());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Label management (unchanged from original)
        // ──────────────────────────────────────────────────────────────────────

        public Label CreateNewLabel(string labelName, Color color)
        {
            int newSliceIndex = _freeIndices.Count > 0 ? _freeIndices.Dequeue() : allLabels.Count;

            // Capture the current band settings from the Artefact
            int rBand = (int)artefact.GetComponent<Renderer>().material.GetFloat(RedBandID);
            int gBand = (int)artefact.GetComponent<Renderer>().material.GetFloat(GreenBandID);
            int bBand = (int)artefact.GetComponent<Renderer>().material.GetFloat(BlueBandID);

            Label newLabel = new Label(labelName, color, newSliceIndex, rBand, gBand, bBand);

            allLabels.Add(newLabel);
            ActivateLabel(newLabel);

            // PREEMPTIVE WIPE: Erase any residual VRAM static before the user can see it
            Graphics.CopyTexture(TextureUtils.GetBlankR8(brush.MaskTexArray.width, brush.MaskTexArray.height), 0, 0,
                brush.MaskTexArray,
                newSliceIndex, 0);

            brush.UpdateShaderVariables();
            Debug.Log($"Created and selected new label: {labelName}");
            return newLabel;
        }

        public void ActivateLabel(Label label)
        {
            activeLabel = label;
            artefact.SetRGBBands(label.rBandIndex, label.gBandIndex, label.bBandIndex);
        }

        public void DeleteLabel(Label labelToDelete)
        {
            if (labelToDelete == null || !allLabels.Contains(labelToDelete)) return;

            allLabels.Remove(labelToDelete);
            _freeIndices.Enqueue(labelToDelete.sliceIndex);

            // ERASE FROM GPU: Instantly copy the blank texture over the specific Z-slice in VRAM
            Graphics.CopyTexture(TextureUtils.GetBlankR8(brush.MaskTexArray.width, brush.MaskTexArray.height), 0, 0,
                brush.MaskTexArray, labelToDelete.sliceIndex, 0);

            brush.UpdateShaderVariables();

            if (activeLabel == labelToDelete)
            {
                if (allLabels.Count > 0)
                {
                    ActivateLabel(allLabels[0]);
                }
                else
                {
                    activeLabel = null;
                }
            }

            // Also delete from backend
            StartCoroutine(DeleteLabelAsync(labelToDelete.id));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Backend persistence
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Extracts the GPU mask for a single label, encodes it as PNG, and POSTs it to the
        /// backend together with the label's metadata. Called by BrushUI when the user clicks Apply.
        /// Updates label.version and label.textureUrl from the backend response.
        /// </summary>
        public void SaveLabel(Label label)
        {
            StartCoroutine(SaveLabelAsync(label));
        }

        private IEnumerator SaveLabelAsync(Label label)
        {
            // 1. Extract the GPU slice for this label into a CPU Texture2D
            RenderTexture sliceRT =
                new RenderTexture(brush.MaskTexArray.width, brush.MaskTexArray.height, 0, RenderTextureFormat.R8);
            Texture2D exportTex =
                TextureUtils.CreateRaw(brush.MaskTexArray.width, brush.MaskTexArray.height, TextureFormat.RGB24);

            Graphics.CopyTexture(brush.MaskTexArray, label.sliceIndex, 0, sliceRT, 0, 0);
            RenderTexture.active = sliceRT;
            exportTex.ReadPixels(new Rect(0, 0, brush.MaskTexArray.width, brush.MaskTexArray.height), 0, 0);
            exportTex.Apply();
            RenderTexture.active = null;

            byte[] pngBytes = exportTex.EncodeToPNG();

            Destroy(sliceRT);
            Destroy(exportTex);

            // 2. Build the metadata JSON — include version so the backend can check optimistic locking
            string metadataJson = BuildMetadataJson(label);

            // 3. Upload
            yield return LabelApiService.UploadLabel(
                artefact.artifactId,
                metadataJson,
                pngBytes,
                onSuccess: dto =>
                {
                    label.id = dto.id;
                    label.version = dto.version;
                    label.textureUrl = dto.textureUrl;
                    Debug.Log($"[LabelManager] Saved label '{label.name}' (version={label.version}).");
                },
                onError: err =>
                {
                    Debug.LogError($"[LabelManager] Failed to save label '{label.name}': {err}");
                }
            );
        }

        private IEnumerator DeleteLabelAsync(string labelId)
        {
            yield return LabelApiService.DeleteLabel(
                labelId,
                onSuccess: () => Debug.Log($"[LabelManager] Deleted label '{labelId}' from backend."),
                onError: err => Debug.LogError($"[LabelManager] Failed to delete label '{labelId}': {err}")
            );
        }

        /// <summary>
        /// Fetches all labels from the backend for this artefact, then downloads and uploads
        /// each label's texture to the GPU. Fires OnLabelsLoaded when done.
        /// Called once from Awake via coroutine.
        /// </summary>
        private IEnumerator LoadSessionAsync()
        {
            List<ArtifactLabelDto> dtos = null;

            yield return LabelApiService.FetchAllLabels(
                artefact.artifactId,
                onSuccess: list => dtos = list,
                onError: err => Debug.LogError($"[LabelManager] LoadSession failed: {err}")
            );

            if (dtos == null || dtos.Count == 0)
            {
                Debug.Log("[LabelManager] No labels found on backend.");
                OnLabelsLoaded.Invoke();
                yield break;
            }

            // Rebuild the in-memory label list from the backend DTOs
            allLabels.Clear();
            foreach (ArtifactLabelDto dto in dtos)
            {
                Label label = new Label(dto.name, dto.color.ToUnityColor(), dto.sliceIndex,
                    dto.rBandIndex, dto.gBandIndex, dto.bBandIndex)
                {
                    id = dto.id,
                    description = dto.description,
                    visible = dto.visible,
                    version = dto.version,
                    textureUrl = dto.textureUrl
                };
                allLabels.Add(label);
            }

            if (allLabels.Count > 0)
            {
                ActivateLabel(allLabels[0]);
                InitializeFreeIndices();
            }

            // Download each label's texture PNG and upload it to the GPU array
            Texture2D formatMatcherTex =
                TextureUtils.CreateRaw(brush.MaskTexArray.width, brush.MaskTexArray.height, TextureFormat.R8);

            foreach (Label label in allLabels)
            {
                if (string.IsNullOrEmpty(label.textureUrl))
                {
                    // No texture on backend yet — upload blank slice
                    Graphics.CopyTexture(
                        TextureUtils.GetBlankR8(brush.MaskTexArray.width, brush.MaskTexArray.height),
                        0, 0, brush.MaskTexArray, label.sliceIndex, 0);
                    continue;
                }

                Texture2D downloadedTex = null;
                yield return LabelApiService.FetchTexture(
                    label.textureUrl,
                    onSuccess: tex => downloadedTex = tex,
                    onError: err =>
                    {
                        Debug.LogWarning($"[LabelManager] Could not fetch texture for '{label.name}': {err}");
                    }
                );

                if (downloadedTex != null)
                {
                    formatMatcherTex.SetPixels32(downloadedTex.GetPixels32());
                    formatMatcherTex.Apply();
                    Graphics.CopyTexture(formatMatcherTex, 0, 0, brush.MaskTexArray, label.sliceIndex, 0);
                    Destroy(downloadedTex);
                }
                else
                {
                    Graphics.CopyTexture(
                        TextureUtils.GetBlankR8(brush.MaskTexArray.width, brush.MaskTexArray.height),
                        0, 0, brush.MaskTexArray, label.sliceIndex, 0);
                }
            }

            Destroy(formatMatcherTex);
            brush.UpdateShaderVariables();

            OnLabelsLoaded.Invoke();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        private void InitializeFreeIndices()
        {
            _freeIndices.Clear();

            HashSet<int> usedIndices = new HashSet<int>();
            int highestIndex = -1;

            foreach (Label label in allLabels)
            {
                usedIndices.Add(label.sliceIndex);
                if (label.sliceIndex > highestIndex)
                {
                    highestIndex = label.sliceIndex;
                }
            }

            for (int i = 0; i < highestIndex; i++)
            {
                if (!usedIndices.Contains(i))
                {
                    _freeIndices.Enqueue(i);
                }
            }
        }

        /// <summary>
        /// Builds the JSON metadata string expected by POST /api/labels/{artifactId}.
        /// Excludes textureUrl and artifactId (set server-side); includes version for optimistic locking.
        /// </summary>
        private static string BuildMetadataJson(Label label)
        {
            // JsonUtility serializes the full Label object, which now includes version.
            // The backend ignores unknown fields and reads version for optimistic locking.
            return JsonUtility.ToJson(label);
        }
    }
}