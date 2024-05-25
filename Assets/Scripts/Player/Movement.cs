using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class Movement : MonoBehaviour
    {
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
            _keyMovement.enabled = false;
            _mouseMovement.enabled = false;
            _orbitingMovement.enabled = false;
        }

        private void Update()
        {
            Cursor.visible = (CursorLockMode.Locked != Cursor.lockState);
            if (Input.GetMouseButton(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                _orbitingMovement.enabled = false;
                _keyMovement.enabled = true;
                _mouseMovement.enabled = true;
                return;
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                _keyMovement.enabled = false;
                _mouseMovement.enabled = false;
                _orbitingMovement.enabled = true;
                return;
            }
            
            Cursor.lockState = CursorLockMode.None;
            _keyMovement.enabled = false;
            _mouseMovement.enabled = false;
            _orbitingMovement.enabled = false;
        }
    }
}