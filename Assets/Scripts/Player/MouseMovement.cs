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
            // Pitch
            // transform.rotation *= Quaternion.AngleAxis(
            //     -Time.deltaTime * Input.GetAxis("Mouse Y") * mouseYSens,
            //     Vector3.right
            // );

            // Yaw
            // transform.rotation = Quaternion.Euler(
            //     transform.eulerAngles.x - Time.deltaTime * mouseYSens * Input.GetAxis("Mouse Y"),
            //     transform.eulerAngles.y + Time.deltaTime * mouseXSens * Input.GetAxis("Mouse X"),
            //     transform.eulerAngles.z
            // );
            // transform.rotation = originalRotation * Quaternion.Euler(pitch, yaw, 0.0f);

            // yaw += Time.deltaTime * mouseXSens * Input.GetAxis("Mouse X");
            // pitch -= Time.deltaTime * mouseYSens * Input.GetAxis("Mouse Y");

            var z = transform.rotation.z;
            transform.Rotate(-Input.GetAxis("Mouse Y") * mouseYSens, 0.0f, 0.0f);
            
            transform.Rotate(0.0f, Input.GetAxis("Mouse X") * mouseXSens, 0.0f);
        }
    }
}