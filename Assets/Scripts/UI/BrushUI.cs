using System;
using System.Collections.Generic;
using UnityEngine;
using ArtefactSystem;
using LabelSystem;
using Pick.Mode;
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
        [SerializeField] private Brush brush;
        
        private readonly Dictionary<int, GameObject> _uiLabels = new();

        private void Start()
        {
            AddUILabels(artefact.Labels);
            artefact.LabelsChanged += ModifyUILabels;
        }

        private void Update()
        {
            return;
        }

        private void AddUILabels(List<Label> labels)
        {
            labels.ForEach(AddUILabel);
        }

        private void AddUILabel(Label label)
        {
            var uiLabel = Instantiate(labelPrefab, contentHolder);

            var uiLabelName = uiLabel.transform.Find("Name").GetComponent<TMP_InputField>();
            uiLabelName.text = label.name;

            var uiLabelDescription = uiLabel.transform.Find("Description").GetComponent<TMP_InputField>();
            uiLabelDescription.text = label.description;

            var uiLabelColor = uiLabel.transform.Find("Color").GetComponent<Image>();
            var uiColor = label.color;
            uiColor.a = 1;
            uiLabelColor.color = uiColor;

            var uiColorField = uiLabel.transform.Find("ColorField").GetComponent<TMP_InputField>();
            uiColorField.text = "#" + ColorUtility.ToHtmlStringRGB(label.color);
            uiColorField.onValueChanged.AddListener(delegate { OnColorFieldChanged(uiColorField, uiLabelColor); });
            uiColorField.onValueChanged.AddListener(delegate { EnsureHashPrefix(uiColorField, uiLabelColor); });

            var uiDeleteButton = uiLabel.transform.Find("DeleteButton").GetComponent<Button>();
            uiDeleteButton.onClick.AddListener(delegate { OnDeleteButtonClick(label.index); });

            var uiVisibleToggle = uiLabel.transform.Find("VisibleToggle").GetComponent<Toggle>();
            uiVisibleToggle.isOn = label.IsVisible();
            uiVisibleToggle.onValueChanged.AddListener(delegate
            {
                OnVisibleToggleChanged(uiVisibleToggle, label.index);
            });

            var uiApplyButton = uiLabel.transform.Find("ApplyButton").GetComponent<Button>();
            uiApplyButton.onClick.AddListener(delegate
            {
                OnApplyButtonClick(label.index, uiLabelName, uiLabelDescription, uiColorField);
            });

            var uiActivateButton = uiLabel.transform.Find("ActivateButton").GetComponent<Button>();
            uiActivateButton.onClick.AddListener(delegate { OnActivateButtonClick(label.index); });

            _uiLabels[label.index] = uiLabel;
        }

        private void OnActivateButtonClick(int labelIndex)
        {
            brush.ActivateLabel(labelIndex);
        }

        public void UpdateActiveLabel(Label label)
        {
            if (!_uiLabels.TryGetValue(label.index, out var uiLabel)) return;
            foreach (var (_, o) in _uiLabels)
            {
                var image = o.GetComponent<Image>();
                image.color = new Color32(255, 255, 255, 100);
            }
            
            var img = uiLabel.GetComponent<Image>();
            img.color = new Color32(255, 0, 0, 100);
        }
        
        private void EnsureHashPrefix(TMP_InputField colorField, Image colorImage)
        {
            if (!colorField.text.StartsWith("#"))
            {
                colorField.text = "#" + colorField.text;
            }
            else if (colorField.text.Length > 1 && colorField.text[0] == '#')
            {
                colorField.onValueChanged.RemoveAllListeners();
                colorField.text = "#" + colorField.text[1..];
                colorField.onValueChanged.AddListener(delegate { OnColorFieldChanged(colorField, colorImage); });
                colorField.onValueChanged.AddListener(delegate { EnsureHashPrefix(colorField, colorImage); });
            }
        }

        private void OnColorFieldChanged(TMP_InputField colorField, Image colorImage)
        {
            if (!ColorUtility.TryParseHtmlString(colorField.text, out var newColor))
            {
                newColor = Color.white;
            }

            newColor.a = 1;
            colorImage.color = newColor;
        }


        private void OnApplyButtonClick(int labelIndex, TMP_InputField uiLabelName, TMP_InputField uiLabelDescription,
            TMP_InputField uiColorField)
        {
            if (!_uiLabels.TryGetValue(labelIndex, out _)) return;
            if (!ColorUtility.TryParseHtmlString(uiColorField.text, out var color))
            {
                Debug.LogError("Invalid color");
                throw new Exception("Invalid color");
            }

            brush.UpdateLabel(labelIndex, uiLabelName.text, uiLabelDescription.text, color);
        }

        private void RemoveUILabels(List<Label> labels)
        {
            labels.ForEach(label =>
            {
                if (!_uiLabels.TryGetValue(label.index, out var uiLabel)) return;
                Destroy(uiLabel);
                _uiLabels.Remove(label.index);
            });
        }

        private void OnDeleteButtonClick(int labelIndex)
        {
            artefact.RemoveLabel(labelIndex);
        }

        private void OnVisibleToggleChanged(Toggle visibleToggle, int labelIndex)
        {
            if (visibleToggle.isOn)
            {
                artefact.ShowLabel(labelIndex);
            }
            else
            {
                artefact.HideLabel(labelIndex);
            }
        }

        public void OnNewLabelClick()
        {
            brush.NewLabel();
        }

        private void ModifyUILabels(object sender, Artefact.LabelsChangedEventArgs args)
        {
            switch (args.Type)
            {
                case LabelEvent.Add:
                {
                    AddUILabels(args.Items);
                    break;
                }
                case LabelEvent.Remove:
                    RemoveUILabels(args.Items);
                    break;
                case LabelEvent.Update:
                    // ModifyUILabels();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}