using System.Collections.Generic;
using System.Linq;
using ArtefactSystem;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BrushUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private Transform contentHolder;
        [SerializeField] private Artefact artefact;
        [SerializeField] private Brush brush;
        [SerializeField] private LabelManager labelManager;
        [SerializeField] private HistogramUI histogramUI;
        
        [Header("Toggles")]
        [SerializeField] private Toggle brushToggle;
        [SerializeField] private Toggle eraserToggle;
        
        // Maps the Label ID to the UI wrapper
        private readonly Dictionary<string, UILabel> _uiLabels = new();
        private UILabel _activeUILabel;

        private void Start()
        {
            // Initialize UI for any labels loaded from the JSON save file
            RefreshAllUILabels();
        }

        /// <summary>
        /// Clears the UI board and instantiates rows for all current labels.
        /// Call this on Start, or if you completely reload a save file.
        /// </summary>
        public void RefreshAllUILabels()
        {
            foreach (var kvp in _uiLabels.Where(kvp => kvp.Value?.Object != null))
            {
                Destroy(kvp.Value.Object);
            }
            _uiLabels.Clear();

            foreach (var label in labelManager.allLabels)
            {
                AddUILabel(label);
            }
        }

        public void OnPaintToggled()
        {
            brush.isErasing = false;
        }

        public void OnEraserToggled()
        {
            brush.isErasing = true;
        }
        
        private void AddUILabel(Label label)
        {
            var uiLabel = new UILabel(labelPrefab, contentHolder)
            {
                Name = { text = label.name },
                Description = { text = label.description },
                Color = { color = ToUIColor(label.color) },
                ColorField = { text = "#" + ColorUtility.ToHtmlStringRGB(label.color) },
                VisibleToggle = { isOn = label.visible } 
            };

            // Setup Listeners
            uiLabel.ColorField.onValueChanged.AddListener(delegate { OnColorFieldChanged(uiLabel.ColorField, uiLabel.Color); });
            uiLabel.ColorField.onValueChanged.AddListener(delegate { EnsureHashPrefix(uiLabel.ColorField, uiLabel.Color); });
            
            uiLabel.DeleteButton.onClick.AddListener(delegate { OnDeleteButtonClick(label); });
            uiLabel.VisibleToggle.onValueChanged.AddListener(delegate { OnVisibleToggleChanged(uiLabel.VisibleToggle, label); });
            
            uiLabel.ApplyButton.onClick.AddListener(delegate { 
                OnApplyButtonClick(label, uiLabel.Name.text, uiLabel.Description.text, uiLabel.ColorField.text); 
            });
            
            uiLabel.ActivateButton.onClick.AddListener(delegate { OnActivateButtonClick(label.id); });
            //TODO
            // uiLabel.GraphButton.onClick.AddListener(delegate { histogramUI.CreateWindow(label.id); });

            _uiLabels[label.id] = uiLabel;
            
            // If this is the active label, highlight it immediately
            if (labelManager.activeLabel == label) OnActivateButtonClick(label.id);
        }

        private void OnActivateButtonClick(string labelId)
        {
            if (!_uiLabels.TryGetValue(labelId, out var uiLabel)) return;

            // Deactivate old label UI visually
            if (_activeUILabel?.Object)
            {
                var image = _activeUILabel.Object.GetComponent<Image>();
                image.color = new Color32(255, 255, 255, 100);
            }

            if (uiLabel == _activeUILabel)
            {
                labelManager.activeLabel = null;
                _activeUILabel = null;
            }
            else
            {
                labelManager.activeLabel = labelManager.allLabels.Find(l => l.id == labelId);
                _activeUILabel = uiLabel;
                
                var img = _activeUILabel.Object.GetComponent<Image>();
                img.color = new Color32(255, 0, 0, 100);
            }
        }

        public void OnNewLabelClick()
        {
            // Create random color for the new label
            Color randomColor = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
            
            // Generate the data
            Label newLabel = labelManager.CreateNewLabel("New Label", randomColor);
            
            // Instantiate the UI
            AddUILabel(newLabel);
        }

        private void OnApplyButtonClick(Label targetLabel, string newName, string newDesc, string hexColor)
        {
            if (!ColorUtility.TryParseHtmlString(hexColor, out var color))
            {
                Debug.LogWarning("Invalid hex color submitted.");
                return;
            }

            // Update the data strictly in RAM
            targetLabel.name = newName;
            targetLabel.description = newDesc;
            targetLabel.color = color;

            // Tell the brush to push the new color to the GPU
            brush.UpdateShaderVariables();
        }

        private void OnVisibleToggleChanged(Toggle visibleToggle, Label targetLabel)
        {
            // Update data
            targetLabel.visible = visibleToggle.isOn;

            // Refresh shader Palette. (If hidden, brush.UpdateShaderVariables will push Color.clear)
            brush.UpdateShaderVariables();
        }

        private void OnDeleteButtonClick(Label labelToDelete)
        {
            if (!_uiLabels.TryGetValue(labelToDelete.id, out var uiLabel)) return;

            // 1. Destroy the UI object
            Destroy(uiLabel.Object);
            _uiLabels.Remove(labelToDelete.id);

            // 2. Use our safe array-shifting delete method in the manager
            labelManager.DeleteLabel(labelToDelete);
            
            // 3. Ensure the UI isn't still "highlighting" a deleted label
            if (labelManager.activeLabel == null && _activeUILabel == uiLabel)
            {
                _activeUILabel = null;
            }
        }

        private static Color ToUIColor(Color color)
        {
            return new Color(color.r, color.g, color.b, 1);
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
    }
}