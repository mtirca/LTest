using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class MouseMovement : MonoBehaviour
    {
        #region UI

        [FormerlySerializedAs("_mouseSens")] [Space] [SerializeField] [Tooltip("Sensitivity of mouse rotation")]
        private float mouseSens = 1.8f;

        #endregion

        void Update()
        {
            RotateCamera();
        }

        private void RotateCamera()
        {
            // Pitch
            transform.rotation *= Quaternion.AngleAxis(
                -Input.GetAxis("Mouse Y") * mouseSens,
                Vector3.right
            );

            // Yaw
            transform.rotation = Quaternion.Euler(
                transform.eulerAngles.x,
                transform.eulerAngles.y + Input.GetAxis("Mouse X") * mouseSens,
                transform.eulerAngles.z
            );
        }
    }
}