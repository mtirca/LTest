using System;
using UnityEngine;

namespace Pick.Mode
{
    public class Picker : MonoBehaviour
    {
        [NonSerialized] public PickMode Value = PickMode.Pixel;
        private PickMode _prevValue;

        private void Awake()
        {
            _prevValue = Value;
        }

        /**
         * Returns whether the pick mode was changed in this frame
         */
        public bool Changed()
        {
            return _prevValue != Value;
        }

        /**
         * Returns whether the pick mode was changed in the last frame from parameter "from" to parameter "to"
         */
        public bool Changed(PickMode from, PickMode to)
        {
            return ChangedFrom(from) && ChangedTo(to);
        }

        /**
        * Returns whether the pick mode was changed in the last frame from parameter "from"
        */
        public bool ChangedFrom(PickMode pickMode)
        {
            return _prevValue == pickMode && Value != pickMode;
        }

        /**
        * Returns whether the pick mode was changed in the last frame to parameter "to"
        */
        public bool ChangedTo(PickMode pickMode)
        {
            return _prevValue != pickMode && Value == pickMode;
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
            _prevValue = Value;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Value = PickMode.Pixel;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Value = PickMode.Curve;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Value = PickMode.Brush;
            }
        }
    }
}