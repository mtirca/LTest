using ArtefactSystem;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    public class OrbitingMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 1.8f;
        [SerializeField] private Artefact artefact;

        private void Update()
        {
            if (!Input.GetMouseButton(0)) return;

            var pos = artefact.GetComponent<Renderer>().bounds.center;

            transform.RotateAround(pos, transform.right, -Input.GetAxis("Mouse Y") * speed);
            transform.RotateAround(pos, transform.up, Input.GetAxis("Mouse X") * speed);
        }
    }
}