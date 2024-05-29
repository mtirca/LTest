using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtefactSystem;
using UnityEngine;

namespace Utils
{
    public static class LabelLoader
    {
        private const string JsonFileName = "labels.json";

        private static readonly string JsonPath = Path.Combine(Application.persistentDataPath, JsonFileName);

        public static void Save(IEnumerable<Label> labels)
        {
            var serializableLabels = new Labels(labels);
            var jsonData = JsonUtility.ToJson(serializableLabels, true);
            File.WriteAllText(JsonPath, jsonData);
        }

        public static IEnumerable<Label> Load()
        {
            try
            {
                var jsonData = File.ReadAllText(JsonPath);
                var serializableData = JsonUtility.FromJson<Labels>(jsonData);
                serializableData.labels.ForEach(label => label.MakeInvisible());
                return serializableData.labels.AsEnumerable();
            }
            catch (FileNotFoundException)
            {
                Debug.Log($"{JsonFileName} not found, creating file containing empty list...");
                var serializableData = new Labels(Enumerable.Empty<Label>());
                File.WriteAllText(JsonPath, JsonUtility.ToJson(serializableData, true));
                return serializableData.labels.AsEnumerable();
            }
        }
    }
}