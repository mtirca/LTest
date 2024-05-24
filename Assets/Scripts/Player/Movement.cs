using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class Movement : MonoBehaviour
    {
        #region UI

        [Space] [SerializeField] [Tooltip("The script is currently active")]
        private bool active = true;

        [Space] [SerializeField] [Tooltip("Camera zooming in/out by 'Mouse Scroll Wheel' is active")]
        private bool enableTranslation = true;

        [SerializeField] [Tooltip("Velocity of camera zooming in/out")]
        private float translationSpeed = 55f;

        [SerializeField] [Tooltip("Camera movement speed")]
        private float movementSpeed = .01f;

        [SerializeField] [Tooltip("Speed of the quick camera movement when holding the 'Left Shift' key")]
        private float boostedSpeed = .05f;

        #endregion UI

        private Vector3 _initPosition;
        private Vector3 _initRotation;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (boostedSpeed < movementSpeed)
                boostedSpeed = movementSpeed;
        }
#endif

        private void Start()
        {
            _initPosition = transform.position;
            _initRotation = transform.eulerAngles;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnEnable()
        {
            if (active)
                Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (!active)
                return;

            if (Cursor.visible)
                return;

            Zoom();

            Move();

            // Return to init position
            if (Input.GetKeyDown(KeyCode.R))
            {
                transform.position = _initPosition;
                transform.eulerAngles = _initRotation;
            }
        }

        private void Move()
        {
            Vector3 deltaPosition = Vector3.zero;
            float currentSpeed = movementSpeed;

            if (Input.GetKey(KeyCode.LeftShift))
                currentSpeed = boostedSpeed;

            if (Input.GetKey(KeyCode.W))
                deltaPosition += transform.forward;

            if (Input.GetKey(KeyCode.S))
                deltaPosition -= transform.forward;

            if (Input.GetKey(KeyCode.A))
                deltaPosition -= transform.right;

            if (Input.GetKey(KeyCode.D))
                deltaPosition += transform.right;

            if (Input.GetKey(KeyCode.Space))
                deltaPosition += transform.up;

            if (Input.GetKey(KeyCode.LeftControl))
                deltaPosition -= transform.up;

            transform.position += deltaPosition * currentSpeed;
        }

        private void Zoom()
        {
            if (enableTranslation)
            {
                transform.Translate(Time.deltaTime * translationSpeed * Input.mouseScrollDelta.y * Vector3.forward);
            }
        }
    }
}