using ArtefactSystem;
using UnityEngine;

namespace LabelSystem
{
    [RequireComponent(typeof(Artefact))]
    public class ShaderLabelUpdater : MonoBehaviour
    {
        [SerializeField] private Artefact artefact;

        private Color[] _colorArray;

        public Color[] ColorArray
        {
            get => _colorArray;
            set
            {
                _colorArray = value;
                artefact.Renderer.material.SetColorArray(_colorArrayId, value);
            }
        }

        private int _labelColorsId;
        private int _colorArrayId;

        private void Awake()
        {
            _colorArrayId = Shader.PropertyToID("_ColorArray");
        }

        private void Start()
        {
            InitColorArray();
        }

        private void Update()
        {
            return;
        }

        private void InitColorArray()
        {
            var colorArray = new Color[32];
            var black = new Color(0, 0, 0, 0);
            for (var i = 0; i < Label.Max; i++)
            {
                var color = artefact.Labels.Find(label => label.index == i)?.color ?? black;
                colorArray[i] = color;
            }

            ColorArray = colorArray;

            // add label indices to vertex color
            artefact.Labels.ForEach(AddVertices);
        }

        /**
         * Updates label in shader to match the color and visibility of label given as parameter
         */
        public void UpdateLabelColor(Label label)
        {
            var colorArray = _colorArray;
            colorArray[label.index] = label.color;
            ColorArray = colorArray;
        }

        /**
         * Hide Labels (make them invisible in shader)
         * Note that this does not make them invisible in application's code
         */
        public void HideLabels()
        {
            var colorArray = _colorArray;
            foreach (var label in artefact.Labels)
            {
                var color = colorArray[label.index];
                colorArray[label.index] = new Color(color.r, color.g, color.b, 0);
            }

            ColorArray = colorArray;
        }

        /**
         * Show Labels (make them visible in shader)
         * Note that this restores the shader's colors' visibilities to match the visibility of labels in application,
         * it does not make all labels visible
         */
        public void ShowLabels()
        {
            var colorArray = _colorArray;
            foreach (var label in artefact.Labels)
            {
                var color = colorArray[label.index];
                colorArray[label.index] = new Color(color.r, color.g, color.b, label.color.a);
            }

            ColorArray = colorArray;
        }
        
        public void RemoveShaderLabel(Label label)
        {
            // Remove labels from shader
            var colorArray = _colorArray;
            colorArray[label.index] = new Color(0, 0, 0, 0);
            ColorArray = colorArray;

            RemoveLabelIndicesFromVerticesColor(label);
        }

        public void AddVertices(Label label)
        {
            var oldColors = artefact.Mesh.colors32;
            var newColors = artefact.Mesh.colors32;

            label.vertices.ForEach(vIndex =>
            {
                var vColor = oldColors[vIndex];
                int lIndex = label.index;

                byte r = vColor.r;
                byte g = vColor.g;
                byte b = vColor.b;
                byte a = vColor.a;

                if (lIndex < 8) r |= (byte)(1 << lIndex);
                else if (lIndex < 16) g |= (byte)(1 << (lIndex - 8));
                else if (lIndex < 24) b |= (byte)(1 << (lIndex - 16));
                else if (lIndex < 32) a |= (byte)(1 << (lIndex - 24));

                newColors[vIndex] = new Color32(r, g, b, a);
            });

            artefact.Mesh.colors32 = newColors;
        }

        private void RemoveLabelIndicesFromVerticesColor(Label label)
        {
            var oldColors = artefact.Mesh.colors32;
            var newColors = artefact.Mesh.colors32;

            label.vertices.ForEach(vIndex =>
            {
                var vColor = oldColors[vIndex];
                int lIndex = label.index;

                byte r = vColor.r;
                byte g = vColor.g;
                byte b = vColor.b;
                byte a = vColor.a;

                if (lIndex < 8) r &= (byte)~(1 << lIndex);
                else if (lIndex < 16) g &= (byte)~(1 << (lIndex - 8));
                else if (lIndex < 24) b &= (byte)~(1 << (lIndex - 16));
                else if (lIndex < 32) a &= (byte)~(1 << (lIndex - 24));

                newColors[vIndex] = new Color32(r, g, b, a);
            });

            artefact.Mesh.colors32 = newColors;
        }
    }
}