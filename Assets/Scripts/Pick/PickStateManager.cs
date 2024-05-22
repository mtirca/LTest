using System;
using UnityEngine;
using UnityEngine.UI;
using Pick.Mode;

namespace Pick
{
    public class PickStateManager : MonoBehaviour
    {
        private Picker _picker;
        
        [SerializeField] private Image crosshair;
        [SerializeField] private GameObject brushPanel;
        [SerializeField] private GameObject pixelCurvePanel;

        private void Awake()
        {
            _picker = GetComponent<Picker>();
            _picker.OnPickModeChanged += LoadPicker;
            LoadCrosshair(_picker.Value);
            ActivatePicker(_picker.Value);
        }

        private void LoadPicker(object sender, Picker.OnPickModeChangedEventArgs args)
        {
            LoadCrosshair(_picker.Value);
            ActivatePicker(_picker.Value);
        }

        //todo move to resources util class
        private string GetCrosshairPath(PickMode pickMode)
        {
            return pickMode switch
            {
                PickMode.Pixel => "Crosshairs/crosshair002",
                PickMode.Curve => "Crosshairs/crosshair001",
                PickMode.Brush => "Crosshairs/crosshair117",
                _ => throw new Exception("Bad picker value")
            };
        }

        //todo move to resources util class
        private void LoadCrosshair(PickMode pickMode)
        {
            var crosshairTex = Resources.Load<Sprite>(GetCrosshairPath(pickMode));
            crosshair.sprite = crosshairTex;
        }

        private void ActivatePicker(PickMode pickMode)
        {
            switch (pickMode)
            {
                case PickMode.Brush:
                    pixelCurvePanel.SetActive(false);
                    brushPanel.SetActive(true);
                    break;
                case PickMode.Pixel or PickMode.Curve:
                    brushPanel.SetActive(false);
                    pixelCurvePanel.SetActive(true);
                    break;
            }
        }
    }
}