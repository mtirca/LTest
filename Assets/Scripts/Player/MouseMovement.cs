using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class MouseMovement : MonoBehaviour
    {
        #region UI

        [SerializeField] [Tooltip("Yaw mouse sensitivity")]
        private float mouseXSens = 1.8f;

        [SerializeField] [Tooltip("Pitch mouse sensitivity")]
        private float mouseYSens = 1.8f;

        #endregion

        private void Update()
        {
            RotateCamera();
        }

        private void RotateCamera()
        {
            transform.Rotate(-Input.GetAxis("Mouse Y") * mouseYSens, 0.0f, 0.0f);
            transform.Rotate(0.0f, Input.GetAxis("Mouse X") * mouseXSens, 0.0f);
        }
    }
}