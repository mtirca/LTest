using Global;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pick.Mode
{
    public class Pixel : MonoBehaviour
    {
        [SerializeField] private Picker picker;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PixelUI ui;
        [SerializeField] private GameObject hitPointPrefab;
        [SerializeField] private StateManager stateManager;

        private GameObject _hitPoint;
        private Vector3 _cursorPos;

        private void OnDisable()
        {
            Destroy(_hitPoint);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) || stateManager.State != State.Cursor ||
                EventSystem.current.IsPointerOverGameObject()) return;

            _cursorPos = Input.mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(_cursorPos);
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (!rend || !rend.sharedMaterial || !rend.sharedMaterial.mainTexture || !meshCollider)
                return;

            // Get color at crosshair
            Texture2D tex = rend.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;
            Color color = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
            // Color square
            ui.ColorSquare.color = color;

            // Add new hit point
            Destroy(_hitPoint);
            _hitPoint = Instantiate(hitPointPrefab, hit.point, Quaternion.identity);
        }
    }
}