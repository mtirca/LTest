using ArtefactSystem;
using Player.Movement;
using UI;
using UnityEngine;

namespace Pick.Mode
{
    public class Sampler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private SamplerUI ui;
        [SerializeField] private GameObject hitPointPrefab;
        [SerializeField] private MovementManager movementManager;

        private GameObject _hitPoint;
        private Vector3 _cursorPos;

        private void OnDisable()
        {
            if (_hitPoint) _hitPoint.SetActive(false);
        }

        private void OnEnable()
        {
            if (_hitPoint) _hitPoint.SetActive(true);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) || movementManager.Movement != Movement.None) return;

            _cursorPos = Input.mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(_cursorPos);
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            Artefact artefact = hit.collider.GetComponent<Artefact>();
            if (!artefact)
            {
                Debug.Log("Hit a non-artefact object");
                return;
            }

            if (!artefact.Renderer || !artefact.Renderer.sharedMaterial ||
                !artefact.Renderer.sharedMaterial.mainTexture || !artefact.MeshCollider)
            {
                Debug.LogError("Artefact does not have a renderer, material, texture or mesh collider");
                return;
            }

            // Get color at crosshair
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= artefact.Texture.width;
            pixelUV.y *= artefact.Texture.height;
            Color color = artefact.Texture.GetPixel((int)pixelUV.x, (int)pixelUV.y);

            ui.SetColor(color);

            // Add new hit point
            Destroy(_hitPoint);
            _hitPoint = Instantiate(hitPointPrefab, hit.point, Quaternion.identity);
        }
    }
}