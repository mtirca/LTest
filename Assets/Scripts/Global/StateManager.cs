using System;
using UnityEngine;

namespace Global
{
    public enum State
    {
        Cursor,
        FreeMovement,
        OrbitingMovement
    }

    public class StateManager : MonoBehaviour
    {
        [SerializeField] private GameObject picker;
        
        private State _state;

        public State State
        {
            get => _state;
            private set
            {
                var oldState = _state;
                _state = value;
                if (oldState == value) return;

                OnGlobalStateChanged?.Invoke(this,
                    new OnGlobalStateChangedEventArgs { OldValue = oldState, NewValue = value });
            }
        }

        public event EventHandler<OnGlobalStateChangedEventArgs> OnGlobalStateChanged;

        public class OnGlobalStateChangedEventArgs
        {
            public State OldValue;
            public State NewValue;
        }
        
        private void Start()
        {
            State = State.Cursor;
        }

        private void Update()
        {
            if (State == State.Cursor && Input.GetKey(KeyCode.LeftAlt))
            {
                State = State.OrbitingMovement;
            }

            if (State == State.OrbitingMovement && Input.GetKeyUp(KeyCode.LeftAlt))
            {
                State = State.Cursor;
            }

            if (State == State.Cursor && Input.GetMouseButton(1))
            {
                State = State.FreeMovement;
            }

            if (State == State.FreeMovement && Input.GetMouseButtonUp(1))
            {
                State = State.Cursor;
            }
        }
    }
}