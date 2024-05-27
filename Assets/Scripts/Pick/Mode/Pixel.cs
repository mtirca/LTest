using UI;
using UnityEngine;

namespace Pick.Mode
{
    public class Pixel : MonoBehaviour
    {
        [SerializeField] private Picker picker;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PixelUI ui;
        [SerializeField] private GameObject hitPointPrefab;
        
        private GameObject _hitPoint;
        private Vector3 _cursorPos;

        private void Start()
        {
            picker.OnPickModeChanged += ClearHitPoint;
        }

        private void ClearHitPoint(object sender, Picker.OnPickModeChangedEventArgs args)
        {
            Destroy(_hitPoint);
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            _cursorPos = Input.mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(_cursorPos);
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null ||
                meshCollider == null)
                return;

            // Get color at crosshair
            Texture2D tex = rend.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;
            Color color = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
            // Color square
            ui.ColorSquare.color = color;

            if (Input.GetMouseButtonDown(0))
            {
                Destroy(_hitPoint);
            }

            // Add new hit point
            _hitPoint = Instantiate(hitPointPrefab, hit.point, Quaternion.identity);
        }
    }
}