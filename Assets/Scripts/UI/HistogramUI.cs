using System;
using System.IO;
using ArtefactSystem;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

namespace UI
{
    [RequireComponent(typeof(BrushUI))]
    public class HistogramUI : MonoBehaviour
    {
        [SerializeField] private Artefact artefact;
        [SerializeField] private GameObject windowPrefab;

        private GameObject _window;
        private LineChart _histogram;
        private int _labelIndex;
        
        public void CreateWindow(int labelIndex)
        {
            Destroy(_window);
            _window = Instantiate(windowPrefab, gameObject.transform);
            _histogram = _window.GetComponentInChildren<LineChart>();
            
            var deleteButton = _window.transform.Find("DeleteButton").GetComponent<Button>();
            deleteButton.onClick.AddListener(OnDeleteButtonClick);

            var saveButton = _window.transform.Find("SaveButton").GetComponent<Button>();
            saveButton.onClick.AddListener(OnSaveButtonClick);

            PopulateHistogram(labelIndex);
        }

        private void OnSaveButtonClick()
        {
            var fileName = $"hist_{_labelIndex}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            var savePath = Path.Combine(Application.persistentDataPath, fileName);
            _histogram.SaveAsImage("png", savePath);
        }

        private void OnDeleteButtonClick()
        {
            Destroy(_window);
            _window = null;
            _histogram = null;
        }

        private void PopulateHistogram(int labelIndex)
        {
            var colors = artefact.GetLabelVerticesColors(labelIndex);

            var rData = new int[256];
            var gData = new int[256];
            var bData = new int[256];

            foreach (var color in colors)
            {
                rData[color.r]++;
                gData[color.g]++;
                bData[color.b]++;
            }

            for (int i = 0; i < 256; i++)
            {
                _histogram.AddData("Red", i, rData[i]);
                _histogram.AddData("Green", i, gData[i]);
                _histogram.AddData("Blue", i, bData[i]);
            }
        }
    }
}