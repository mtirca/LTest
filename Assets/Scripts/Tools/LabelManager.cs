using System.Collections.Generic;
using System.IO;
using LabelSystem.JsonPersister;
using UnityEngine;

namespace Tools
{
    public class LabelManager : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Brush brushTool;

        [Header("State")] public List<Label> allLabels = new();
        public Label activeLabel;

        private static string SaveDirectory => Application.persistentDataPath + "/ArtefactLabels";
        private static string JsonPath => SaveDirectory + "/labels.json";
        private static string PngPath => SaveDirectory + "/mask.png";

        private void Start()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            LoadSession();
        }

        public void CreateNewLabel(string labelName, Color color)
        {
            Label newLabel = new Label(labelName, color);
            allLabels.Add(newLabel);
            activeLabel = newLabel;

            Debug.Log($"Created and selected new label: {labelName}");
        }

        public void SaveSession()
        {
            // Save the Metadata to JSON
            Labels db = new Labels { labels = allLabels };
            string json = JsonUtility.ToJson(db, true);
            File.WriteAllText(JsonPath, json);

            // Save the Painted Mask to PNG
            // EncodeToPNG() grabs the raw pixels from the RAM and compresses them
            byte[] pngData = brushTool.MaskTexture.EncodeToPNG();
            File.WriteAllBytes(PngPath, pngData);

            Debug.Log($"Session saved to: {SaveDirectory}");
        }

        private void LoadSession()
        {
            // 1. Load the Metadata
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                Labels db = JsonUtility.FromJson<Labels>(json);
                allLabels = db.labels;

                if (allLabels.Count > 0)
                {
                    activeLabel = allLabels[0];
                }
            }

            // 2. Load the Painted Mask
            if (File.Exists(PngPath))
            {
                byte[] pngData = File.ReadAllBytes(PngPath);
                brushTool.MaskTexture.LoadImage(pngData);
            }
        }
    }
}