using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

        private void Start()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            LoadSession();
        }

        public Label CreateNewLabel(string labelName, Color color)
        {
            int newSliceIndex = _freeIndices.Count > 0 ? _freeIndices.Dequeue() : allLabels.Count;
            Label newLabel = new Label(labelName, color, newSliceIndex, brush.maskWidth, brush.maskHeight);
            allLabels.Add(newLabel);
            activeLabel = newLabel;
            brush.UpdateShaderVariables();
            Debug.Log($"Created and selected new label: {labelName}");
            return newLabel;
        }

        public void DeleteLabel(Label labelToDelete)
        {
            if (labelToDelete == null)
            {
                Debug.Log("Cannot delete null label");
                return;
            }

            if (!allLabels.Contains(labelToDelete))
            {
                Debug.LogWarning($"Trying to delete label {labelToDelete.name}, but it's not present in memory.");
                return;
            }

            allLabels.Remove(labelToDelete);

            // Delete the slice on the GPU
            Color32[] blankPixels = new Color32[brush.maskWidth * brush.maskHeight];
            for (int i = 0; i < blankPixels.Length; i++)
            {
                blankPixels[i] = new Color32(0, 0, 0, 0);
            }

            _freeIndices.Enqueue(labelToDelete.sliceIndex);
            
            brush.MaskTexArray.SetPixels32(blankPixels, labelToDelete.sliceIndex);
            brush.MaskTexArray.Apply();

            brush.UpdateShaderVariables();

            if (activeLabel == labelToDelete)
            {
                activeLabel = allLabels.Count > 0 ? allLabels[0] : null;
            }
        }


        /// <summary>
        /// Saves the labels' metadata in a JSON file and saves the labels' textures.
        /// </summary>
        public void SaveSession()
        {
            // Save the Metadata to JSON
            Labels db = new Labels { labels = allLabels };
            string json = JsonUtility.ToJson(db, true);
            File.WriteAllText(JsonPath, json);

            // Save individual masks
            Texture2D tempTex = new Texture2D(brush.maskWidth, brush.maskHeight, TextureFormat.R8, false, true);
            foreach (Label label in allLabels)
            {
                tempTex.SetPixels32(label.Pixels);
                byte[] pngData = tempTex.EncodeToPNG();

                File.WriteAllBytes(ComputeLabelPath(label.id), pngData);
            }

            Destroy(tempTex);
        }


        /// <summary>
        /// Loads the labels' metadata from the JSON file and the labels' textures from disk.
        /// </summary>
        private void LoadSession()
        {
            // Load labels from disk
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                Labels db = JsonUtility.FromJson<Labels>(json);
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

            // Load PNGs back into the labels
            foreach (Label label in allLabels)
            {
                string labelMaskPath = ComputeLabelPath(label.id);
                if (File.Exists(labelMaskPath))
                {
                    Texture2D tempTex = new Texture2D(2, 2);
                    byte[] pngData = File.ReadAllBytes(labelMaskPath);
                    tempTex.LoadImage(pngData);
                    label.Pixels = tempTex.GetPixels32();
                    Destroy(tempTex);
                }
                else
                {
                    Debug.LogWarning($"Missing PNG for label {label.name}. Generating blank mask.");
                    label.Pixels = new Color32[brush.maskWidth * brush.maskHeight];
                    for (int i = 0; i < label.Pixels.Length; i++)
                    {
                        label.Pixels[i] = new Color32(0, 0, 0, 0);
                    }
                }
                brush.MaskTexArray.SetPixels32(label.Pixels, label.sliceIndex);
            }

            brush.MaskTexArray.Apply();
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