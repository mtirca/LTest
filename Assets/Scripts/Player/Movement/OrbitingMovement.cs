using ArtefactSystem;
using UnityEngine;

namespace Player.Movement
{
    [RequireComponent(typeof(Camera))]
    public class OrbitingMovement : MonoBehaviour
    {
        [SerializeField] private float speed = 1.8f;
        [SerializeField] private Artefact artefact;

        private void Update()
        {
            if (!Input.GetMouseButton(0)) return;

            var center = artefact.GetComponent<Renderer>().bounds.center;

            transform.RotateAround(center, transform.right, -Input.GetAxis("Mouse Y") * speed);
            transform.RotateAround(center, transform.up, Input.GetAxis("Mouse X") * speed);
        }
    }
}