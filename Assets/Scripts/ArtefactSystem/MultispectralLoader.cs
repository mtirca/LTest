using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ArtefactSystem
{
    public class MultispectralLoader : MonoBehaviour
    {
        [Header("Target")] [SerializeField] private Artefact artefact;

        [Header("Source Data")] [Tooltip("The relative path to the images inside the Unity project")]
        public string folderPath = "Assets/Artefact/Texture/image_folder";

        public string fileExtension = "*.png";

        private struct BandData : IComparable<BandData>
        {
            public int wavelength;
            //todo needed only if we create the MS tex programatically, probably
            public string filePath;

            public int CompareTo(BandData other)
            {
                return wavelength.CompareTo(other.wavelength);
            }
        }

        /// <summary>
        /// Right-click this component in the Unity Inspector to run this method!
        /// </summary>
        [ContextMenu("1. Parse Folder and Load Wavelengths")]
        public void LoadWavelengths()
        {
            if (artefact == null)
            {
                Debug.LogError("Please assign the Target Artefact!");
                return;
            }

            if (string.IsNullOrWhiteSpace(folderPath) || !folderPath.StartsWith("Assets"))
            {
                Debug.LogError("<b>Invalid Path:</b> The folder path must point to a path inside this project. The path must start with 'Assets/'.");
                return;
            }

            string resolvedPath = Application.dataPath + folderPath.Substring(6);
            if (!Directory.Exists(resolvedPath))
            {
                Debug.LogError($"Directory not found: {resolvedPath}");
                return;
            }

            string[] filePaths = Directory.GetFiles(folderPath, fileExtension);
            var validBands = new List<BandData>();

            // This Regex looks for any number of digits (\d+) immediately followed by "nm"
            Regex regex = new Regex(@"(\d+)nm", RegexOptions.IgnoreCase);

            foreach (string path in filePaths)
            {
                string fileName = Path.GetFileName(path);
                Match match = regex.Match(fileName);

                if (match.Success)
                {
                    int parsedWavelength = int.Parse(match.Groups[1].Value);

                    validBands.Add(new BandData
                    {
                        wavelength = parsedWavelength,
                        filePath = path
                    });
                }
            }

            if (validBands.Count == 0)
            {
                Debug.LogWarning("No files matching the '*nm' pattern were found.");
                return;
            }

            validBands.Sort();

            artefact.Wavelengths = new int[validBands.Count];
            for (int i = 0; i < validBands.Count; i++)
            {
                artefact.Wavelengths[i] = validBands[i].wavelength;
            }

            // This marks artifact as dirty inside the Editor to force a save. Not sure if we actually need this. todo
            #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(artefact);
            #endif
            
            Debug.Log($"Successfully loaded and sorted {validBands.Count} wavelengths into the Artefact!");
        }
    }
}