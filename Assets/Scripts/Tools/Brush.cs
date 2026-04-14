using ArtefactSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tools
{
    public class Brush : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Camera mainCamera;

        [SerializeField] private Artefact artefact;
        [SerializeField] private LabelManager labelManager;

        [Header("Brush Settings")] public int maskWidth = 2048;
        public int maskHeight = 1536;
        public int brushRadius = 15;
        public bool isErasing = false;

        private Texture2D _maskTexture;
        private Renderer _artefactRenderer;

        public Texture2D MaskTexture => _maskTexture;

        private static readonly int LabelMaskID = Shader.PropertyToID("_LabelMask");

        private void Awake()
        {
            InitializeBlankCanvas();
        }

        /// <summary>
        /// Finds all pixels of the old color and replaces them with the new color.
        /// </summary>
        public void ReplaceColorInMask(Color oldColor, Color newColor)
        {
            Color[] pixels = _maskTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                // Tolerance check for floating point color inaccuracies
                if (Mathf.Abs(pixels[i].r - oldColor.r) < 0.01f && 
                    Mathf.Abs(pixels[i].g - oldColor.g) < 0.01f && 
                    Mathf.Abs(pixels[i].b - oldColor.b) < 0.01f)
                {
                    pixels[i] = newColor;
                }
            }
            _maskTexture.SetPixels(pixels);
            _maskTexture.Apply();
        }

        /// <summary>
        /// Erases all paint associated with a specific color.
        /// </summary>
        public void DeleteColorFromMask(Color colorToDelete)
        {
            ReplaceColorInMask(colorToDelete, Color.clear);
        }
        
        private void InitializeBlankCanvas()
        {
            _maskTexture = new Texture2D(maskWidth, maskHeight, TextureFormat.RGBA32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] clearPixels = new Color[maskWidth * maskHeight];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = Color.clear;
            }

            _maskTexture.SetPixels(clearPixels);
            _maskTexture.Apply();

            _artefactRenderer = artefact.GetComponent<Renderer>();
            _artefactRenderer.material.SetTexture(LabelMaskID, _maskTexture);
        }

        private void Update()
        {
            //todo check sampler.cs and make the same check at the beginning
            if (!Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                PaintSurface();
            }
        }

        private void PaintSurface()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.gameObject != artefact.gameObject)
            {
                return;
            }

            Vector2 uv = hit.textureCoord;

            int pixelX = Mathf.FloorToInt(uv.x * maskWidth);
            int pixelY = Mathf.FloorToInt(uv.y * maskHeight);

            DrawCircleOnTexture(pixelX, pixelY);
        }

        private void DrawCircleOnTexture(int centerX, int centerY)
        {
            Color paintColor;
            if (isErasing)
            {
                paintColor = Color.clear;
            }
            else if (labelManager.activeLabel != null)
            {
                paintColor = labelManager.activeLabel.color;
            }
            else
            {
                return;
            }

            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                for (int y = -brushRadius; y <= brushRadius; y++)
                {
                    if (x * x + y * y <= brushRadius * brushRadius)
                    {
                        int drawX = centerX + x;
                        int drawY = centerY + y;

                        if (drawX >= 0 && drawX < maskWidth && drawY >= 0 && drawY < maskHeight)
                        {
                            _maskTexture.SetPixel(drawX, drawY, paintColor);
                        }
                    }
                }
            }

            _maskTexture.Apply();
        }
    }
}