using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utils;

namespace Tools
{
    public class LabelManager : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Brush brush;

        [Header("State")] public List<Label> allLabels = new();
        public Label activeLabel;

        // Queue that holds the free label indices in the shader attributes
        private readonly Queue<int> _freeIndices = new();

        private static string SaveDirectory => Application.persistentDataPath + "/ArtefactLabels";
        private static string JsonPath => SaveDirectory + "/labels.json";

        private void Awake()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            LoadSession();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("Auto-saving Artefact Session before quitting...");
            SaveSession();
        }

        public Label CreateNewLabel(string labelName, Color color)
        {
            int newSliceIndex = _freeIndices.Count > 0 ? _freeIndices.Dequeue() : allLabels.Count;

            Label newLabel = new Label(labelName, color, newSliceIndex, brush.maskWidth, brush.maskHeight);
            allLabels.Add(newLabel);
            activeLabel = newLabel;

            // PREEMPTIVE WIPE: Erase any residual VRAM static before the user can see it
            Graphics.CopyTexture(TextureUtils.GetBlankR8(brush.maskWidth, brush.maskHeight), 0, 0, brush.MaskTexArray,
                newSliceIndex, 0);

            brush.UpdateShaderVariables();
            Debug.Log($"Created and selected new label: {labelName}");
            return newLabel;
        }

        public void DeleteLabel(Label labelToDelete)
        {
            if (labelToDelete == null || !allLabels.Contains(labelToDelete)) return;

            allLabels.Remove(labelToDelete);
            _freeIndices.Enqueue(labelToDelete.sliceIndex);

            // ERASE FROM GPU: Instantly copy the blank texture over the specific Z-slice in VRAM
            Graphics.CopyTexture(TextureUtils.GetBlankR8(brush.maskWidth, brush.maskHeight), 0, 0,
                brush.MaskTexArray, labelToDelete.sliceIndex, 0);

            brush.UpdateShaderVariables();

            if (activeLabel == labelToDelete)
            {
                activeLabel = allLabels.Count > 0 ? allLabels[0] : null;
            }
        }

        /// <summary>
        /// Saves labels to JSON on disk and grabs the textures from VRAM, copies them to RAM and saves them on disk.
        /// </summary>
        public void SaveSession()
        {
            Labels db = new Labels { labels = allLabels };
            File.WriteAllText(JsonPath, JsonUtility.ToJson(db, true));

            // We need a 2D RenderTexture to extract slices, and an RGB24 Texture2D to encode to PNG
            RenderTexture sliceExtractionRT =
                new RenderTexture(brush.maskWidth, brush.maskHeight, 0, RenderTextureFormat.R8);
            Texture2D exportTex = TextureUtils.CreateRaw(brush.maskWidth, brush.maskHeight, TextureFormat.RGB24);

            foreach (Label label in allLabels)
            {
                // 1. Copy the specific 3D slice out of the Array into the flat 2D RenderTexture
                Graphics.CopyTexture(brush.MaskTexArray, label.sliceIndex, 0, sliceExtractionRT, 0, 0);

                // 2. Download the VRAM from the 2D RenderTexture into the CPU Texture2D
                RenderTexture.active = sliceExtractionRT;
                exportTex.ReadPixels(new Rect(0, 0, brush.maskWidth, brush.maskHeight), 0, 0);
                exportTex.Apply();

                // 3. Encode to disk
                File.WriteAllBytes(ComputeLabelPath(label.id), exportTex.EncodeToPNG());
            }

            // Cleanup
            RenderTexture.active = null;
            Destroy(sliceExtractionRT);
            Destroy(exportTex);
        }

        /// <summary>
        /// Loads the labels' metadata from the JSON file and the labels' textures from disk.
        /// </summary>
        private void LoadSession()
        {
            // Load labels from disk
            if (File.Exists(JsonPath))
            {
                Labels db = JsonUtility.FromJson<Labels>(File.ReadAllText(JsonPath));
                allLabels = db.labels;

                if (allLabels.Count > 0)
                {
                    activeLabel = allLabels[0];
                    InitializeFreeIndices();
                }
            }
            else
            {
                //todo remove this later, should not have any active label and press on UI on plus to create new or sth
                Debug.Log("No labels found on disk. Initializing a default label...");
                CreateNewLabel("Red", new Color(1, 0, 0, 1));
            }

            // Texture used to match the format of the GPU Array before copying
            Texture2D formatMatcherTex = TextureUtils.CreateRaw(brush.maskWidth, brush.maskHeight, TextureFormat.R8);

            foreach (Label label in allLabels)
            {
                string labelMaskPath = ComputeLabelPath(label.id);
                if (File.Exists(labelMaskPath))
                {
                    // 1. Load the PNG from disk into a temporary texture (LoadImage creates an RGBA texture)
                    Texture2D tempLoadedPng = new Texture2D(2, 2);
                    tempLoadedPng.LoadImage(File.ReadAllBytes(labelMaskPath));

                    // 2. Transfer pixels to our R8 format matcher
                    formatMatcherTex.SetPixels32(tempLoadedPng.GetPixels32());
                    formatMatcherTex.Apply();

                    // 3. Upload instantly to the GPU array slice
                    Graphics.CopyTexture(formatMatcherTex, 0, 0, brush.MaskTexArray, label.sliceIndex, 0);

                    Destroy(tempLoadedPng);
                }
                else
                {
                    // If missing, upload blank
                    Graphics.CopyTexture(TextureUtils.GetBlankR8(brush.maskWidth, brush.maskHeight), 0, 0,
                        brush.MaskTexArray, label.sliceIndex, 0);
                }
            }

            Destroy(formatMatcherTex);
            brush.UpdateShaderVariables();
        }

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

        private static string ComputeLabelPath(string labelId)
        {
            return SaveDirectory + $"/mask_{labelId}.png";
        }
    }
}