using System.Collections.Generic;
using ArtefactSystem;
using UnityEngine;

namespace LabelSystem
{
    [RequireComponent(typeof(Artefact))]
    public class ShaderLabelUpdater : MonoBehaviour
    {
        private Artefact _artefact;

        private Color[] _colorArray;

        public Color[] ColorArray
        {
            get => _colorArray;
            set
            {
                _colorArray = value;
                _artefact.Renderer.material.SetColorArray(_colorArrayId, value);
            }
        }

        private int _labelColorsId;
        private int _colorArrayId;

        private void Awake()
        {
            _artefact = GetComponent<Artefact>();
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
                var color = _artefact.Labels.Find(label => label.index == i)?.color ?? black;
                colorArray[i] = color;
            }

            ColorArray = colorArray;

            // add label indices to vertex color
            _artefact.Labels.ForEach(AddVertices);
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
         * Updates labels in shader to match the colors and visibilities of labels given as parameter
         */
        public void UpdateLabelColors(List<Label> labels)
        {
            var colorArray = _colorArray;
            labels.ForEach(label =>
            {
                colorArray[label.index] = label.color;
            });
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
            var oldColors = _artefact.Mesh.colors32;
            var newColors = _artefact.Mesh.colors32;

            label.vertices.ForEach(labelVertex =>
            {
                int vIndex = labelVertex.index;
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

            _artefact.Mesh.colors32 = newColors;
        }

        private void RemoveLabelIndicesFromVerticesColor(Label label)
        {
            var oldColors = _artefact.Mesh.colors32;
            var newColors = _artefact.Mesh.colors32;
            
            label.vertices.ForEach(labelVertex =>
            {
                int vIndex = labelVertex.index;
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

            _artefact.Mesh.colors32 = newColors;
        }
    }
}