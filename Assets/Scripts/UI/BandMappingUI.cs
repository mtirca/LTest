using System.Collections.Generic;
using ArtefactSystem;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class BandMappingUI : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Artefact artefact;
        [SerializeField] private LabelManager labelManager;

        [Header("UI Hierarchy")]
        [Tooltip("The visual panel containing the dropdowns that will be hidden/shown.")]
        [SerializeField] private GameObject contentPanel;

        [Tooltip("The button that stays permanently on screen to open/close the panel.")] 
        [SerializeField] private Button toggleVisibilityButton;

        [Tooltip("The text component inside the toggle button.")] 
        [SerializeField] private TextMeshProUGUI toggleButtonText;

        [Header("UI Elements")] 
        [SerializeField] private TMP_Dropdown redDropdown;
        [SerializeField] private TMP_Dropdown greenDropdown;
        [SerializeField] private TMP_Dropdown blueDropdown;
        [SerializeField] private Button resetButton;

        private void Start()
        {
            InitializeDropdowns();
            InitializeResetButton();
            InitializeVisibilityToggle();
            UpdateToggleText();
        }

        private void InitializeDropdowns()
        {
            if (artefact.Wavelengths == null || artefact.Wavelengths.Length == 0)
            {
                Debug.LogWarning("No wavelengths found on Artefact.");
                return;
            }

            List<string> options = new List<string>();
            foreach (int wavelength in artefact.Wavelengths)
            {
                options.Add($"{wavelength} nm");
            }

            SetupDropdown(redDropdown, options);
            SetupDropdown(greenDropdown, options);
            SetupDropdown(blueDropdown, options);

            ResetToDefaults();
        }

        private void InitializeResetButton()
        {
            if (resetButton == null)
            {
                Debug.LogWarning("The reset button on the band mapping panel is not initialized.");
                return;
            }

            resetButton.onClick.AddListener(ResetToDefaults);
        }

        private void InitializeVisibilityToggle()
        {
            if (toggleVisibilityButton == null)
            {
                Debug.LogWarning("The visibility toggle button on the band mapping panel is not initialized.");
                return;
            }

            toggleVisibilityButton.onClick.AddListener(TogglePanelVisibility);
        }

        public void ResetToDefaults()
        {
            // Set RGB dropdowns to their actual wavelengths by default
            redDropdown.value = MSUtils.GetClosestWavelengthIndex(artefact.Wavelengths, 650);
            greenDropdown.value = MSUtils.GetClosestWavelengthIndex(artefact.Wavelengths, 550);
            blueDropdown.value = MSUtils.GetClosestWavelengthIndex(artefact.Wavelengths, 475);

            // Push to the shader
            PushBandsToShader();
        }

        private void SetupDropdown(TMP_Dropdown dropdown, List<string> options)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(delegate { PushBandsToShader(); });
        }
        
        /// <summary>
        /// Flips the active state of the visual panel.
        /// </summary>
        public void TogglePanelVisibility()
        {
            if (contentPanel == null) 
            {
                Debug.LogWarning("The content panel on the band mapping panel is not initialized.");
                return;
            }

            contentPanel.SetActive(!contentPanel.activeSelf);
            
            UpdateToggleText();
        }
        
        private void UpdateToggleText()
        {
            if (contentPanel == null) 
            {
                Debug.LogWarning("The content panel on the band mapping panel is not initialized.");
                return;
            }

            if (toggleButtonText == null)
            {
                Debug.LogWarning("The toggle button text on the band mapping panel is not initialized.");
                return;
            }
            
            toggleButtonText.text = contentPanel.activeSelf ? "Close" : "Bands";
        }

        private void PushBandsToShader()
        {
            artefact.SetRGBBands(redDropdown.value, greenDropdown.value, blueDropdown.value);
        }

        public void SetDropdownValues(int rIndex, int gIndex, int bIndex)
        {
            redDropdown.value = rIndex;
            greenDropdown.value = gIndex;
            blueDropdown.value = bIndex;
            
            // Disable dropdowns if a label is active
            bool isLabelActive = labelManager.activeLabel != null;
            redDropdown.interactable = !isLabelActive;
            greenDropdown.interactable = !isLabelActive;
            blueDropdown.interactable = !isLabelActive;
            resetButton.interactable = !isLabelActive;
        }
    }
}