using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class KeyMovement : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = .01f;
        [SerializeField] private float boostedSpeed = .05f;
        [SerializeField] private float rotationSens = 100.0f;

        private void Update()
        {
            Vector3 deltaPos = Vector3.zero;
            float currSpeed = movementSpeed;

            if (Input.GetKey(KeyCode.LeftShift))
                currSpeed = boostedSpeed;

            if (Input.GetKey(KeyCode.W))
                deltaPos += transform.forward;

            if (Input.GetKey(KeyCode.S))
                deltaPos -= transform.forward;

            if (Input.GetKey(KeyCode.A))
                deltaPos -= transform.right;

            if (Input.GetKey(KeyCode.D))
                deltaPos += transform.right;
            
            if (Input.GetKey(KeyCode.Space))
                deltaPos += transform.up;
            
            if (Input.GetKey(KeyCode.LeftControl))
                deltaPos -= transform.up;

            transform.position += deltaPos * currSpeed;

            if (Input.GetKey(KeyCode.E))
                transform.Rotate(0.0f, 0.0f, -Time.deltaTime * rotationSens);
            
            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(0.0f, 0.0f, +Time.deltaTime * rotationSens);
        }
    }
}