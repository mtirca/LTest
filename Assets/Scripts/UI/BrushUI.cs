using System;
using System.Collections.Generic;
using UnityEngine;
using ArtefactSystem;
using LabelSystem;
using TMPro;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

namespace UI
{
    public class BrushUI : MonoBehaviour
    {
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private Transform contentHolder;
        [SerializeField] private Artefact artefact;

        private List<GameObject> _uiLabels = new();
        
        private void Start()
        {
            AddUILabels(artefact.Labels);
            artefact.LabelsChanged += UpdateUILabels;
        }

        private void Update()
        {
            return;
        }

        private void AddUILabels(List<Label> labels)
        {
            labels.ForEach(newLabel =>
            {
                var uiLabel = Instantiate(labelPrefab, contentHolder);
                
                var uiLabelName = uiLabel.transform.Find("Name").GetComponent<TMP_InputField>();
                uiLabelName.text = newLabel.description;
                
                var uiLabelDescription = uiLabel.transform.Find("Description").GetComponent<TMP_InputField>();
                uiLabelDescription.text = newLabel.description;

                var uiColorField = uiLabel.transform.Find("ColorField").GetComponent<TMP_InputField>();
                uiColorField.text = ColorUtility.ToHtmlStringRGB(newLabel.color);

                var uiLabelColor = uiLabel.transform.Find("Color").GetComponent<Image>();
                var uiColor = newLabel.color;
                uiColor.a = 1;
                uiLabelColor.color = uiColor;
                
                _uiLabels.Add(uiLabel);
            });
        }

        private void UpdateUILabels(object sender, Artefact.LabelsChangedEventArgs args)
        {
            switch (args.Type)
            {
                case LabelEvent.Add:
                {
                    AddUILabels(args.Items);
                    break;
                }
                case LabelEvent.Remove:
                    //todo update if remove label
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}