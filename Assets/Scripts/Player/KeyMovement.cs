using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class KeyMovement : MonoBehaviour
    {
        #region UI

        [SerializeField] [Tooltip("Camera zooming in/out by 'Mouse Scroll Wheel' is active")]
        private bool enableTranslation = true;

        [SerializeField] [Tooltip("Velocity of camera zooming in/out")]
        private float translationSpeed = 10f;

        [SerializeField] [Tooltip("Camera movement speed")]
        private float movementSpeed = .01f;

        [SerializeField] [Tooltip("Speed of the quick camera movement when holding the 'Left Shift' key")]
        private float boostedSpeed = .05f;

        [SerializeField] [Tooltip("The sensitivity of the rotation around own axis")]
        private float rotationSens = 100.0f;
        
        #endregion UI

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (boostedSpeed < movementSpeed)
                boostedSpeed = movementSpeed;
        }
#endif

        private void Update()
        {
            Zoom();

            Move();
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

            if (Input.GetKey(KeyCode.E))
                transform.Rotate(0.0f, 0.0f, -Time.deltaTime * rotationSens);
            
            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(0.0f, 0.0f, +Time.deltaTime * rotationSens);
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