using System;
using Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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

        [SerializeField] private ToolManager toolManager;

        private KeyMovement _keyMovement;
        private MouseMovement _mouseMovement;
        private OrbitingMovement _orbitingMovement;
        private RollMovement _rollMovement;

        //todo remove
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
            Movement = Movement.None;
        }

        private void UpdateMovementScripts(Movement movement)
        {
            if (toolManager)
            {
                toolManager.enabled = movement == Movement.None;
            }

            Cursor.visible = movement == Movement.None;

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
            bool isHoveringUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();

            // Orbit Logic (Alt)
            if (Movement == Movement.None && Input.GetKey(KeyCode.LeftAlt) && !isHoveringUI)
            {
                Movement = Movement.Orbit;
            }

            if (Movement == Movement.Orbit && Input.GetKeyUp(KeyCode.LeftAlt))
            {
                Movement = Movement.None;
            }

            // FreeLook Logic (Right Click)
            if (Movement == Movement.None && Input.GetMouseButton(1) && !isHoveringUI)
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