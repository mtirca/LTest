using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LabelSystem.Utils
{
    public static class LabelJsonPersister
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
            Labels serializableData;
            try
            {
                var jsonData = File.ReadAllText(JsonPath);
                serializableData = JsonUtility.FromJson<Labels>(jsonData);
            }
            catch (FileNotFoundException)
            {
                Debug.Log($"{JsonFileName} not found, creating file containing empty list...");
                serializableData = new Labels(Enumerable.Empty<Label>());
                File.WriteAllText(JsonPath, JsonUtility.ToJson(serializableData, true));
            }

            return serializableData.labels.AsEnumerable();
        }
    }
}