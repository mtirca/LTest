using UnityEngine;

namespace Player.Movement
{
    [RequireComponent(typeof(Camera))]
    public class MouseMovement : MonoBehaviour
    {
        [SerializeField] private float mouseXSens = 1.8f;
        [SerializeField] private float mouseYSens = 1.8f;

        private void Update()
        {
            transform.Rotate(-Input.GetAxis("Mouse Y") * mouseYSens, 0.0f, 0.0f);
            transform.Rotate(0.0f, Input.GetAxis("Mouse X") * mouseXSens, 0.0f);
        }
    }
}