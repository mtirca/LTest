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
        [SerializeField] private HistogramUI histogramUI;
        [SerializeField] private Toggle brushToggle;
        [SerializeField] private Toggle eraserToggle;
        
        private readonly Dictionary<int, UILabel> _uiLabels = new();

        private UILabel _activeLabel;

        private void Start()
        {
            AddUILabels(artefact.Labels);
        }

        private void AddUILabels(List<Label> labels)
        {
            labels.ForEach(AddUILabel);
        }

        private static Color ToUIColor(Color color)
        {
            return new Color(color.r, color.g, color.b, 1);
        }

        public void OnPaintToggled()
        {
            brush.ActivatePaint();
        }

        public void OnEraserToggled()
        {
            brush.ActivateEraser();
        }
        
        private void AddUILabel(Label label)
        {
            var uiLabel = new UILabel(labelPrefab, contentHolder)
            {
                Name =
                {
                    text = label.name
                },
                Description =
                {
                    text = label.description
                },
                Color =
                {
                    color = ToUIColor(label.color)
                },
                ColorField =
                {
                    text = "#" + ColorUtility.ToHtmlStringRGB(label.color)
                },
                VisibleToggle =
                {
                    isOn = label.IsVisible()
                }
            };

            uiLabel.ColorField.onValueChanged.AddListener(delegate
            {
                OnColorFieldChanged(uiLabel.ColorField, uiLabel.Color);
            });
            uiLabel.ColorField.onValueChanged.AddListener(delegate
            {
                EnsureHashPrefix(uiLabel.ColorField, uiLabel.Color);
            });
            uiLabel.DeleteButton.onClick.AddListener(delegate { OnDeleteButtonClick(label.index); });
            uiLabel.VisibleToggle.onValueChanged.AddListener(delegate
            {
                OnVisibleToggleChanged(uiLabel.VisibleToggle, label.index);
            });
            uiLabel.ApplyButton.onClick.AddListener(delegate
            {
                OnApplyButtonClick(label.index, uiLabel.Name, uiLabel.Description, uiLabel.ColorField);
            });
            uiLabel.ActivateButton.onClick.AddListener(delegate { OnActivateButtonClick(label.index); });
            uiLabel.GraphButton.onClick.AddListener(delegate { OnGenerateGraphButtonClick(label.index); });

            _uiLabels[label.index] = uiLabel;
        }

        private void OnGenerateGraphButtonClick(int labelIndex)
        {
            histogramUI.CreateWindow(labelIndex);
        }

        /**
         * If the active label is pressed, it is deactivated
         * If another label is pressed, the current active label is also deactivated, and the pressed label is activated
         */
        private void OnActivateButtonClick(int labelIndex)
        {
            if (!_uiLabels.TryGetValue(labelIndex, out var uiLabel)) return;

            // Deactivate old label
            if (_activeLabel?.Object)
            {
                var image = _activeLabel.Object.GetComponent<Image>();
                image.color = new Color32(255, 255, 255, 100);
            }

            if (uiLabel == _activeLabel)
            {
                brush.ClearActiveLabel();
                _activeLabel = null;
            }
            else // Activate new label
            {
                brush.ActivateLabel(labelIndex);
                _activeLabel = uiLabel;
                var img = _activeLabel.Object.GetComponent<Image>();
                img.color = new Color32(255, 0, 0, 100);
            }
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
            foreach (var label in labels)
            {
                if (!_uiLabels.TryGetValue(label.index, out var uiLabel)) return;
                Destroy(uiLabel.Object);
                _uiLabels.Remove(label.index);
            }
        }

        private void UpdateUILabels(List<Label> labels)
        {
            foreach (var label in labels)
            {
                if (!_uiLabels.TryGetValue(label.index, out var uiLabel)) return;
                uiLabel.Name.text = label.name;
                uiLabel.Description.text = label.description;
                uiLabel.Color.color = ToUIColor(label.color);
            }
        }

        private void OnDeleteButtonClick(int labelIndex)
        {
            brush.RemoveLabel(labelIndex);
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

        private void UpdateVisibleToggleUI(List<Label> labels)
        {
            foreach (var label in labels)
            {
                if (!_uiLabels.TryGetValue(label.index, out var uiLabel)) return;
                uiLabel.VisibleToggle.isOn = label.IsVisible();
            }
        }

        public void OnNewLabelClick()
        {
            brush.NewLabel();
        }

        public void ModifyUILabels(LabelEvent labelEvent, List<Label> labels)
        {
            switch (labelEvent)
            {
                case LabelEvent.Add:
                    AddUILabels(labels);
                    break;
                case LabelEvent.Remove:
                    RemoveUILabels(labels);
                    break;
                case LabelEvent.VisibleUpdate:
                    UpdateVisibleToggleUI(labels);
                    break;
                case LabelEvent.Update:
                    UpdateUILabels(labels);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(labelEvent));
            }
        }
    }
}