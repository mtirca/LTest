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
            crosshair.sprite = Utils.Resource.LoadCrosshair(_picker.Value);
            ActivatePicker(_picker.Value);
        }

        private void LoadPicker(object sender, Picker.OnPickModeChangedEventArgs args)
        {
            crosshair.sprite = Utils.Resource.LoadCrosshair(_picker.Value);
            ActivatePicker(_picker.Value);
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