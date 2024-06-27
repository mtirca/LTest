using ArtefactSystem;
using Global;
using UI;
using UnityEngine;

namespace Pick.Mode
{
    public class Pixel : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PixelUI ui;
        [SerializeField] private GameObject hitPointPrefab;
        [SerializeField] private StateManager stateManager;

        private GameObject _hitPoint;
        private Vector3 _cursorPos;

        private void OnDisable()
        {
            _hitPoint?.SetActive(false);
        }

        private void OnEnable()
        {
            _hitPoint?.SetActive(true);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) || stateManager.State != State.Cursor) return;

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