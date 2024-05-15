using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pick.Mode
{
    [RequireComponent(typeof(Camera))]
    public class PixelCurve : MonoBehaviour
    {
        [SerializeField] private Image colorSquare;
        [SerializeField] private GameObject hitPointPrefab;

        private readonly List<GameObject> _hitPoints = new();

        private Picker _picker;
        
        private void Awake()
        {
            _picker = Camera.main.GetComponent<Picker>();
        }

        void Update()
        {
            if (_picker.PickModeChanged())
            {
                ClearHitPoints();
            }
            
            //todo move pixel and curve logic to separate classes
            bool pointClicked = Input.GetMouseButtonDown(0) && _picker.Value == PickMode.Pixel;
            bool curveHeld = Input.GetMouseButton(0) && _picker.Value == PickMode.Curve;
            if (!pointClicked && !curveHeld) return;

            Camera mainCamera = Camera.main;
            Vector3 centerPixel = mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
            Ray ray = mainCamera.ScreenPointToRay(centerPixel);
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
            colorSquare.color = color;

            if (Input.GetMouseButtonDown(0))
            {
                ClearHitPoints();
            }

            // Add new hit point
            var hitPoint = Instantiate(hitPointPrefab, hit.point, Quaternion.identity);
            _hitPoints.Add(hitPoint);
        }

        private void ClearHitPoints()
        {
            foreach (var hitMarker in _hitPoints)
            {
                Destroy(hitMarker);
            }

            _hitPoints.Clear();
        }
    }
}