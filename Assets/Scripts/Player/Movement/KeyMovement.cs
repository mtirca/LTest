using UnityEngine;

namespace Player.Movement
{
    [RequireComponent(typeof(Camera))]
    public class KeyMovement : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 3.0f;

        private void Update()
        {
            Vector3 deltaPos = Vector3.zero;
            float currSpeed = movementSpeed;

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

            transform.position += deltaPos * (Time.deltaTime * currSpeed);
        }
    }
}