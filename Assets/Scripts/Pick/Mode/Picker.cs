using System;
using Global;
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
                if (stateManager.State != State.Cursor) return;

                var oldValue = _value;
                if (oldValue == value) return;

                _value = value;

                ActivatePickModeScripts(_value);
                OnPickModeChanged?.Invoke(this,
                    new OnPickModeChangedEventArgs { OldValue = oldValue, NewValue = _value });
            }
        }

        [SerializeField] private GameObject pixel;
        [SerializeField] private GameObject pixelUI;
        [SerializeField] private GameObject brush;
        [SerializeField] private GameObject brushUI;

        public event EventHandler<OnPickModeChangedEventArgs> OnPickModeChanged;

        public class OnPickModeChangedEventArgs
        {
            public PickMode OldValue;
            public PickMode NewValue;
        }

        private void Start()
        {
            ActivatePickModeScripts(_value);
            OnPickModeChanged?.Invoke(this,
                new OnPickModeChangedEventArgs { OldValue = _value, NewValue = _value });
        }

        private void ActivatePickModeScripts(PickMode newValue)
        {
            switch (newValue)
            {
                case PickMode.None:
                    brush.SetActive(false);
                    brushUI.SetActive(false);
                    pixel.SetActive(false);
                    pixelUI.SetActive(false);
                    break;
                case PickMode.Pixel:
                    brush.SetActive(false);
                    brushUI.SetActive(false);
                    pixel.SetActive(true);
                    pixelUI.SetActive(true);
                    break;
                case PickMode.Brush:
                    pixel.SetActive(false);
                    pixelUI.SetActive(false);
                    brush.SetActive(true);
                    brushUI.SetActive(true);
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