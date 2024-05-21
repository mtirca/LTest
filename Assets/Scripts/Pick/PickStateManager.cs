using System;
using UnityEngine;
using UnityEngine.UI;
using Pick.Mode;

namespace Pick
{
    [RequireComponent(typeof(Image))]
    public class PickStateManager : MonoBehaviour
    {
        private Picker _picker;
        private Image _crosshair;
        //todo panels work?
        [SerializeField] private GameObject brushPanel;
        [SerializeField] private GameObject pixelCurvePanel;

        private void Awake()
        {
            _crosshair = gameObject.GetComponent<Image>();
            _picker = Camera.main.GetComponent<Picker>();
            _picker.OnPickModeChanged += ReloadUI;
            LoadCrosshair(_picker.Value);
            LoadPickerPanel(_picker.Value);
        }

        private void ReloadUI(object sender, Picker.OnPickModeChangedEventArgs args)
        {
            LoadCrosshair(_picker.Value);
            LoadPickerPanel(_picker.Value);
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
            _crosshair.sprite = crosshairTex;
        }

        private void LoadPickerPanel(PickMode pickMode)
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