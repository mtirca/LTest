using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
 [RequireComponent(typeof(Camera))]
 public class MouseMovement : MonoBehaviour
 {
     #region UI
 
     [FormerlySerializedAs("_active")] [Space] [SerializeField] [Tooltip("The script is currently active")]
     private bool active = true;
     
     [FormerlySerializedAs("_mouseSens")] [Space] [SerializeField] [Tooltip("Sensitivity of mouse rotation")]
     private float mouseSens = 1.8f;
 
     #endregion
     
     void Start()
     {
         Cursor.lockState = CursorLockMode.Locked;
     }
 
     void Update()
     {
         if (!active)
             return;
         
         SetCursorState();
         
         RotateCamera();
     }
     
     // Apply requested cursor state
     private void SetCursorState()
     {
         // ESC
         if (Input.GetKeyDown(KeyCode.Escape))
         {
             Cursor.lockState = CursorLockMode.None;
         }
 
         // LMB
         if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
         {
             Cursor.lockState = CursorLockMode.Locked;
         }
 
         // Hide cursor when locking
         Cursor.visible = (CursorLockMode.Locked != Cursor.lockState);
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
