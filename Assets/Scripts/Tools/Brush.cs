using System.Linq;
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
        //todo these should be defined somewhere artefact-related
        public int maskHeight = 1536;
        public int brushRadius = 15;
        public bool isErasing;

        [Header("Advanced Rendering")] public int maxLabels = 64;

        private Texture2D _paletteTexture;
        private Renderer _artefactRenderer;
        private Texture2D _activeCanvas;
        private Label _lastPaintedLabel;
        
        public Texture2DArray MaskTexArray { get; private set; }

        private static readonly int MaskArrayID = Shader.PropertyToID("_MaskArray");
        private static readonly int PaletteID = Shader.PropertyToID("_Palette");
        private static readonly int ActiveCountID = Shader.PropertyToID("_LabelCount");

        private void Awake()
        {
            InitializeGPUArrays();
        }

        private void InitializeGPUArrays()
        {
            MaskTexArray = new Texture2DArray(maskWidth, maskHeight, maxLabels, TextureFormat.R8, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            // Initialize the Palette Texture (64x1 pixels)
            _paletteTexture = new Texture2D(maxLabels, 1, TextureFormat.RGBA32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _activeCanvas = new Texture2D(maskWidth, maskHeight, TextureFormat.R8, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            
            _artefactRenderer = artefact.GetComponent<Renderer>();
            _artefactRenderer.material.SetTexture(MaskArrayID, MaskTexArray);
            _artefactRenderer.material.SetTexture(PaletteID, _paletteTexture);

            UpdateShaderVariables();
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
            if (labelManager.activeLabel == null)
            {
                return;
            }

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
            Label activeLabel = labelManager.activeLabel;
            
            // Sync the active canvas if the user selected a new label
            //todo ??? whats this
            if (activeLabel != _lastPaintedLabel)
            {
                _activeCanvas.SetPixels32(activeLabel.Pixels);
                _activeCanvas.Apply();
                _lastPaintedLabel = activeLabel;
            }
            
            // 1. Calculate the exact bounding box of the brush stroke
            //todo this works for 2D textures on 2D planes. but wont this logic not work for 3d objects where their texture can be scrambled? because no one
            // can guarantee that on the texture if you go left 2 pixels, its the same as going left on the object
            int startX = Mathf.Clamp(centerX - brushRadius, 0, maskWidth - 1);
            int startY = Mathf.Clamp(centerY - brushRadius, 0, maskHeight - 1);
            int endX = Mathf.Clamp(centerX + brushRadius, 0, maskWidth - 1);
            int endY = Mathf.Clamp(centerY + brushRadius, 0, maskHeight - 1);

            int blockWidth = endX - startX + 1;
            int blockHeight = endY - startY + 1;
            
            // We only create an array for the pixels inside the brush box (~900 pixels instead of 3 Million)
            Color32[] blockColors = new Color32[blockWidth * blockHeight];
            
            bool textureChanged = false;
            //todo extract clear and red
            Color32 paintValue = isErasing ? new Color32(0, 0, 0, 0) : new Color32(255, 0, 0, 0);

            for (int y = 0; y < blockHeight; y++)
            {
                for (int x = 0; x < blockWidth; x++)
                {
                    int worldX = startX + x;
                    int worldY = startY + y;
                    int flatIndex = worldY * maskWidth + worldX;

                    int dx = worldX - centerX;
                    int dy = worldY - centerY;

                    // If inside the circle, apply paint
                    if (dx * dx + dy * dy <= brushRadius * brushRadius)
                    {
                        if (activeLabel.Pixels[flatIndex].r != paintValue.r)
                        {
                            activeLabel.Pixels[flatIndex] = paintValue;
                            textureChanged = true;
                        }
                    }
                    
                    // Copy the state of the RAM into our tiny block array
                    blockColors[y * blockWidth + x] = activeLabel.Pixels[flatIndex];
                }
            }

            if (textureChanged)
            {
                // Push ONLY the tiny block to the GPU
                _activeCanvas.SetPixels32(startX, startY, blockWidth, blockHeight, blockColors);
                _activeCanvas.Apply();
                
                // Instantly transfer the 2D texture directly into the Array slice purely on the GPU
                Graphics.CopyTexture(_activeCanvas, 0, 0, MaskTexArray, activeLabel.sliceIndex, 0);
            }
        }
        
        /// <summary>
        /// To be called whenever a label is created, deleted, or changes color.
        /// </summary>
        public void UpdateShaderVariables()
        {
            // Tell the shader the highest index we are currently using, so it doesn't loop unnecessarily (performance reasons only)
            int highestIndex = labelManager.allLabels.Select(label => label.sliceIndex).Prepend(0).Max();
            _artefactRenderer.material.SetFloat(ActiveCountID, highestIndex + 1);

            // Update the Nx1 color palette
            for (int i = 0; i < maxLabels; i++)
            {
                _paletteTexture.SetPixel(i, 0, Color.clear);
            }
            foreach (var label in labelManager.allLabels.Where(label => label.visible))
            {
                _paletteTexture.SetPixel(label.sliceIndex, 0, label.color);
            }
            _paletteTexture.Apply();
        }
    }
}