using System.Collections.Generic;
using System.IO;
using ArtefactSystem;
using UnityEngine;

namespace LabelSystem
{
    [RequireComponent(typeof(Artefact))]
    public class ShaderLabelUpdater : MonoBehaviour
    {
        private Artefact _artefact;

        private Texture2D _encodedLabels;
        public Texture2D EncodedLabels
        {
            get => _encodedLabels;
            set
            {
                _encodedLabels = value;
                _artefact.Renderer.material.SetTexture(_labelColorsId, value);
                File.WriteAllBytes(@"C:\Users\mihne\AppData\LocalLow\DefaultCompany\LTest\tex.png", (_artefact.Renderer.material.GetTexture(_labelColorsId) as Texture2D).EncodeToPNG()); 
            }
        }

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
            // _labelColorsId = Shader.PropertyToID("_Labels");
            _colorArrayId = Shader.PropertyToID("_ColorArray");
        }

        private void Start()
        {
            // InitEncodedLabels();
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
        
        /**
         * Inits the label texture in shader:
         * Label colors are set in corresponding pixels
         * The empty pixels are set to black
         */
        private void InitEncodedLabels()
        {
            var initTex = new Texture2D(Label.Max, 1, TextureFormat.RGBA32, false, true)
            {
                filterMode = FilterMode.Point
            };
            var black = new Color32(0, 0, 0, 0);
            for (var i = 0; i < Label.Max; i++)
            {
                var color = _artefact.Labels.Find(label => label.index == i)?.color ?? black;
                initTex.SetPixel(i, 0, color);
            }
            
            initTex.Apply();
            EncodedLabels = initTex;
            
            // add label indices to vertex color
            AddLabelIndicesToVerticesColor(_artefact.Labels);
        }
        
        public void AddLabelsToShader(List<Label> labels)
        {
            // Add labels to encoded labels texture
            var encodedLabels = _encodedLabels;
            labels.ForEach(label => encodedLabels.SetPixel(label.index, 0, label.color));
            EncodedLabels = encodedLabels;

            // add label indices to vertex color
            AddLabelIndicesToVerticesColor(labels);
        }

        public void AddLabelsToShaderX(List<Label> labels)
        {
            // Add labels to encoded labels texture
            var colorArray = _colorArray;
            foreach (var label in labels)
            {
                colorArray[label.index] = label.color;
            }
            ColorArray = colorArray;

            // add label indices to vertex color
            AddLabelIndicesToVerticesColor(labels);
        }
        
        public void RemoveLabelsFromShader(List<Label> labels)
        {
            // Remove labels from encoded labels texture
            var labelColors = _encodedLabels;
            var black = new Color(0, 0, 0, 0);
            labels.ForEach(label => labelColors.SetPixel(label.index, 0, black));
            EncodedLabels = labelColors;

            // Remove label indices from vertex color
            RemoveLabelIndicesFromVerticesColor(labels);
        }

        private void AddLabelIndicesToVerticesColor(List<Label> labels)
        {
            var vColors = _artefact.Mesh.colors32;
            labels.ForEach(label =>
            {
                label.vertices.ForEach(labelVertex =>
                {
                    int vIndex = labelVertex.index;
                    var vColor = vColors[vIndex];
                    int lIndex = label.index;

                    byte r = vColor.r;
                    byte g = vColor.g;
                    byte b = vColor.b;
                    byte a = vColor.a;

                    if (lIndex < 8) r |= (byte)(1 << lIndex);
                    else if (lIndex < 16) g |= (byte)(1 << (lIndex - 8));
                    else if (lIndex < 24) b |= (byte)(1 << (lIndex - 16));
                    else if (lIndex < 32) a |= (byte)(1 << (lIndex - 24));

                    var newVColor = new Color32(r, g, b, a);
                    var newColors = _artefact.Mesh.colors32;
                    newColors[vIndex] = newVColor;
                    _artefact.Mesh.colors32 = newColors;
                });
            });
        }

        private void RemoveLabelIndicesFromVerticesColor(List<Label> labels)
        {
            var vColors = _artefact.Mesh.colors32;
            labels.ForEach(label =>
            {
                label.vertices.ForEach(labelVertex =>
                {
                    int vIndex = labelVertex.index;
                    var vColor = vColors[vIndex];
                    int lIndex = label.index;

                    byte r = vColor.r;
                    byte g = vColor.g;
                    byte b = vColor.b;
                    byte a = vColor.a;

                    if (lIndex < 8) r &= (byte)~(1 << lIndex);
                    else if (lIndex < 16) g &= (byte)~(1 << (lIndex - 8));
                    else if (lIndex < 24) b &= (byte)~(1 << (lIndex - 16));
                    else if (lIndex < 32) a &= (byte)~(1 << (lIndex - 24));

                    var newVColor = new Color32(r, g, b, a);
                    _artefact.Mesh.colors32[vIndex] = newVColor;
                });
            });
        }
    }
}