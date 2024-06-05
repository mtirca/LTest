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
            AddLabelIndicesToVerticesColor(_artefact.Labels);
        }

        public void RemoveShaderLabels(List<Label> labels)
        {
            // Remove labels from shader
            var colorArray = _colorArray;
            foreach (var label in labels)
            {
                colorArray[label.index] = new Color(0, 0, 0, 0);
            }

            ColorArray = colorArray;

            RemoveLabelIndicesFromVerticesColor(labels);
        }

        public void UpdateShaderLabels(List<Label> labels)
        {
            // Add labels to shader
            var colorArray = _colorArray;
            foreach (var label in labels)
            {
                colorArray[label.index] = label.color;
            }

            ColorArray = colorArray;

            AddLabelIndicesToVerticesColor(labels);
        }

        private void AddLabelIndicesToVerticesColor(List<Label> labels)
        {
            var oldColors = _artefact.Mesh.colors32;
            var newColors = _artefact.Mesh.colors32;
            labels.ForEach(label =>
            {
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
            });

            _artefact.Mesh.colors32 = newColors;
        }

        private void RemoveLabelIndicesFromVerticesColor(List<Label> labels)
        {
            var oldColors = _artefact.Mesh.colors32;
            var newColors = _artefact.Mesh.colors32;
            labels.ForEach(label =>
            {
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
            });

            _artefact.Mesh.colors32 = newColors;
        }
    }
}