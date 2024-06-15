using System;
using Global;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class MovementManager : MonoBehaviour
    {
        [SerializeField] private StateManager stateManager;

        private KeyMovement _keyMovement;
        private MouseMovement _mouseMovement;
        private OrbitingMovement _orbitingMovement;

        private void Awake()
        {
            _keyMovement = GetComponent<KeyMovement>();
            _mouseMovement = GetComponent<MouseMovement>();
            _orbitingMovement = GetComponent<OrbitingMovement>();
        }

        private void Start()
        {
            HandleStateMovement(stateManager.State);
            stateManager.OnGlobalStateChanged += MovementChanged;
        }

        private void MovementChanged(object sender, StateManager.OnGlobalStateChangedEventArgs args)
        {
            HandleStateMovement(args.NewValue);
        }

        private void HandleStateMovement(State state)
        {
            switch (state)
            {
                case State.Cursor:
                    _keyMovement.enabled = false;
                    _mouseMovement.enabled = false;
                    _orbitingMovement.enabled = false;
                    break;
                case State.FreeMovement:
                    _keyMovement.enabled = true;
                    _mouseMovement.enabled = true;
                    _orbitingMovement.enabled = false;
                    break;
                case State.OrbitingMovement:
                    _keyMovement.enabled = false;
                    _mouseMovement.enabled = false;
                    _orbitingMovement.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}