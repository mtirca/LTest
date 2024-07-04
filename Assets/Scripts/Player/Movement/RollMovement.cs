using UnityEngine;

namespace Player.Movement
{
    [RequireComponent(typeof(Camera))]
    public class RollMovement : MonoBehaviour
    {
        [SerializeField] private float rotationSens = 100.0f;

        private void Update()
        {
            if (Input.GetKey(KeyCode.E))
                transform.Rotate(0.0f, 0.0f, -Time.deltaTime * rotationSens);
            
            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(0.0f, 0.0f, Time.deltaTime * rotationSens);
        }
    }
}