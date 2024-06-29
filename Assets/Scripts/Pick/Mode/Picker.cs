using System;
using Player;
using UI;
using UnityEngine;

namespace Pick.Mode
{
    public class Picker : MonoBehaviour
    {
        [SerializeField] private StateManager stateManager;

        private PickMode _value = PickMode.None;

        public PickMode Value
        {
            get => _value;
            private set
            {
                var oldValue = _value;
                if (oldValue == value) return;

                _value = value;

                ActivatePickModeScripts(_value);
                OnPickModeChanged?.Invoke(this,
                    new OnPickModeChangedEventArgs { OldValue = oldValue, NewValue = _value });
            }
        }

        [SerializeField] private Pixel pixel;
        [SerializeField] private PixelUI pixelUI;
        [SerializeField] private Brush brush;
        [SerializeField] private BrushUI brushUI;
        [SerializeField] private FreeCursorUI freeCursorUI;

        public event EventHandler<OnPickModeChangedEventArgs> OnPickModeChanged;

        public class OnPickModeChangedEventArgs
        {
            public PickMode OldValue;
            public PickMode NewValue;
        }

        private void ActivatePickModeScripts(PickMode newValue)
        {
            switch (newValue)
            {
                case PickMode.None:
                    brush.gameObject.SetActive(false);
                    brushUI.gameObject.SetActive(false);
                    pixel.gameObject.SetActive(false);
                    pixelUI.gameObject.SetActive(false);
                    freeCursorUI.gameObject.SetActive(true);
                    break;
                case PickMode.Pixel:
                    brush.gameObject.SetActive(false);
                    brushUI.gameObject.SetActive(false);
                    pixel.gameObject.SetActive(true);
                    pixelUI.gameObject.SetActive(true);
                    freeCursorUI.gameObject.SetActive(false);
                    break;
                case PickMode.Brush:
                    pixel.gameObject.SetActive(false);
                    pixelUI.gameObject.SetActive(false);
                    brush.gameObject.SetActive(true);
                    brushUI.gameObject.SetActive(true);
                    freeCursorUI.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        private void Update()
        {
            ChangePickMode();
        }

        /**
         * Changes picker mode if key was pressed
         */
        private void ChangePickMode()
        {
            if (stateManager.State != State.Cursor) return;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Value = PickMode.None;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Value = PickMode.Pixel;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Value = PickMode.Brush;
            }
        }
    }
}