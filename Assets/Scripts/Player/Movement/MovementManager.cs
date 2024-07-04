using System;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Movement
{
    public enum Movement
    {
        None,
        FreeLook,
        Orbit
    }
    
    [RequireComponent(typeof(Camera))]
    public class MovementManager : MonoBehaviour
    {
        private Movement _movement;
        public Movement Movement
        {
            get => _movement;
            private set
            {
                _movement = value;
                UpdateMovementScripts(value);
                onGlobalStateChanged?.Invoke(value);
            }
        }
        
        private KeyMovement _keyMovement;
        private MouseMovement _mouseMovement;
        private OrbitingMovement _orbitingMovement;
        private RollMovement _rollMovement;
        
        public UnityEvent<Movement> onGlobalStateChanged;
        
        private void Awake()
        {
            _keyMovement = GetComponent<KeyMovement>();
            _mouseMovement = GetComponent<MouseMovement>();
            _orbitingMovement = GetComponent<OrbitingMovement>();
            _rollMovement = GetComponent<RollMovement>();
        }

        private void Start()
        {
            _movement = Movement.None;
        }

        private void UpdateMovementScripts(Movement movement)
        {
            switch (movement)
            {
                case Movement.None:
                    _keyMovement.enabled = false;
                    _mouseMovement.enabled = false;
                    _orbitingMovement.enabled = false;
                    _rollMovement.enabled = false;
                    break;
                case Movement.FreeLook:
                    _keyMovement.enabled = true;
                    _mouseMovement.enabled = true;
                    _orbitingMovement.enabled = false;
                    _rollMovement.enabled = true;
                    break;
                case Movement.Orbit:
                    _keyMovement.enabled = false;
                    _mouseMovement.enabled = false;
                    _orbitingMovement.enabled = true;
                    _rollMovement.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (Movement == Movement.None && Input.GetKey(KeyCode.LeftAlt))
            {
                Movement = Movement.Orbit;
            }

            if (Movement == Movement.Orbit && Input.GetKeyUp(KeyCode.LeftAlt))
            {
                Movement = Movement.None;
            }

            if (Movement == Movement.None && Input.GetMouseButton(1))
            {
                Movement = Movement.FreeLook;
            }

            if (Movement == Movement.FreeLook && Input.GetMouseButtonUp(1))
            {
                Movement = Movement.None;
            }
        }
    }
}