using Other;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class OrbitingMovement : MonoBehaviour
    {
        [SerializeField] [Tooltip("Sensitivity of mouse rotation")]
        private float speed = 1.8f;
        
        private void Update()
        {
            var axis1 = transform.right;
            var axis2 = transform.up;
            
            var pos = Artefact.Instance.GetComponent<Renderer>().bounds.center;

            transform.RotateAround(pos, axis1,
                -Input.GetAxis("Mouse Y") * speed);
            transform.RotateAround(pos, axis2,
                Input.GetAxis("Mouse X") * speed);
        }
    }
}