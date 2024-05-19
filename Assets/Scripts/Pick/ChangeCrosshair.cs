using System;
using UnityEngine;
using UnityEngine.UI;
using Pick.Mode;

namespace Pick
{
    [RequireComponent(typeof(Image))]
    public class ChangeCrosshair : MonoBehaviour
    {
        private Image _crosshair;
        private Picker _picker;
        
        private void Awake()
        {
            _crosshair = gameObject.GetComponent<Image>();
            _picker = Camera.main.GetComponent<Picker>();
            LoadCrosshair();
        }

        void Update()
        {
            if (!_picker.Changed()) return;
            LoadCrosshair();
        }

        private string GetCrosshairPath()
        {
            return _picker.Value switch
            {
                PickMode.Pixel => "Crosshairs/crosshair002",
                PickMode.Curve => "Crosshairs/crosshair001",
                PickMode.Brush => "Crosshairs/crosshair117",
                _ => throw new Exception("Bad picker value")
            };
        }

        private void LoadCrosshair()
        {
            Sprite crosshairTex = Resources.Load<Sprite>(GetCrosshairPath());
            _crosshair.sprite = crosshairTex;
        }
    }
}