using System.Collections.Generic;
using ArtefactSystem;
using UnityEngine;

namespace LabelSystem
{
    [RequireComponent(typeof(Artefact))]
    public class ShaderLabelUpdater : MonoBehaviour
    {
        [SerializeField] private Artefact artefact;

        private Color[] _colorArray;
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
        
        private void InitColorArray()
        {
            var colorArray = new Color[32];
            var black = new Color(0, 0, 0, 0);
            for (var i = 0; i < Label.Max; i++)
            {
                var color = artefact.Labels.Find(label => label.id == i)?.color ?? black;
                colorArray[i] = color;
            }

            _colorArray = colorArray;
            artefact.Renderer.material.SetColorArray(_colorArrayId, _colorArray);

            // add label indices to vertex color
            artefact.Labels.ForEach(AddVertices);
        }

        /**
         * Updates label in shader to match the color and visibility of label given as parameter
         */
        public void UpdateLabelColor(Label label)
        {
            _colorArray[label.id] = label.color;
            artefact.Renderer.material.SetColorArray(_colorArrayId, _colorArray);
        }

        /**
         * Hide Labels (make them invisible in shader)
         * Note that this does not make them invisible in application's code
         */
        public void HideLabels()
        {
            if (_colorArray == null)
            {
                return;
            }
            foreach (var label in artefact.Labels)
            {
                _colorArray[label.id].a = 0;
            }

            artefact.Renderer.material.SetColorArray(_colorArrayId, _colorArray);
        }

        /**
         * Show Labels (make them visible in shader)
         * Note that this restores the shader's colors' visibilities to match the visibility of labels in application,
         * it does not make all labels visible
         */
        public void ShowLabels()
        {
            if (_colorArray == null)
            {
                return;
            }
            foreach (var label in artefact.Labels)
            {
                _colorArray[label.id].a = label.color.a;
            }

            artefact.Renderer.material.SetColorArray(_colorArrayId, _colorArray);
        }
        
        public void RemoveShaderLabel(Label label)
        {
            _colorArray[label.id] = new Color(0, 0, 0, 0);
            artefact.Renderer.material.SetColorArray(_colorArrayId, _colorArray);
            
            RemoveVertices(label);
        }

        private void AddVertices(Label label)
        {
            AddVertices(label.vertices, label.id);
        }
        
        public void AddVertices(List<int> vertices, int lIndex)
        {
            var oldColors = artefact.Mesh.colors32;
            
            // HACK 1: If Unity gave us an empty array, initialize it to the correct size
            if (oldColors == null || oldColors.Length == 0)
            {
                oldColors = new Color32[artefact.Mesh.vertexCount];
            }
            
            var newColors = oldColors;

            vertices.ForEach(vIndex =>
            {
                // HACK 2: Ignore stale save data that points to non-existent vertices
                if (vIndex < 0 || vIndex >= oldColors.Length) return; 

                var vColor = oldColors[vIndex];

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

        private void RemoveVertices(Label label)
        {
            RemoveVertices(label.vertices, label.id);
        }
        
        public void RemoveVertices(List<int> vertices, int lIndex)
        {
            var oldColors = artefact.Mesh.colors32;
            
            if (oldColors == null || oldColors.Length == 0)
            {
                oldColors = new Color32[artefact.Mesh.vertexCount];
            }
            
            var newColors = oldColors;

            vertices.ForEach(vIndex =>
            {
                if (vIndex < 0 || vIndex >= oldColors.Length) return;

                var vColor = oldColors[vIndex];

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