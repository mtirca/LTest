using System.Collections.Generic;
using System.IO;
using Tools;
using UnityEngine;

namespace LabelSystem.JsonPersister
{
    //todo fix and use or delete
    public static class LabelJsonPersister
    {
        private const string JsonFileName = "labels.json";

        private static readonly string JsonPath = Path.Combine(Application.persistentDataPath, JsonFileName);

        public static void Save(List<Label> labels)
        {
            var serializableLabels = new Labels {labels = labels};
            var jsonData = JsonUtility.ToJson(serializableLabels, true);
            File.WriteAllText(JsonPath, jsonData);
        }

        public static List<Label> Load()
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
                serializableData = new Labels();
                File.WriteAllText(JsonPath, JsonUtility.ToJson(serializableData, true));
            }

            return serializableData.labels;
        }
    }
}