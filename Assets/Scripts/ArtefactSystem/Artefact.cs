using System;
using System.Collections.Generic;
using System.Linq;
using LabelSystem;
using LabelSystem.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace ArtefactSystem
{
    public class Artefact : MonoBehaviour
    {
        public Renderer Renderer { get; private set; }
        private MeshFilter MeshFilter { get; set; }
        public Mesh Mesh => MeshFilter.sharedMesh;
        public MeshCollider MeshCollider { get; private set; }

        public Texture2D Texture
        {
            get => Renderer.material.mainTexture as Texture2D;
            set => Renderer.material.mainTexture = value;
        }

        public List<Label> Labels { get; private set; }

        public ShaderLabelUpdater ShaderUpdater { get; private set; }

        public UnityEvent<LabelEvent, List<Label>> labelsChanged;

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
            ShaderUpdater = GetComponent<ShaderLabelUpdater>();
            InitVertexColors();
            InitLabels();
        }

        /**
         * <returns>The Colors of the vertices of label with labelIndex</returns>
         */
        public List<Color32> GetLabelVerticesColors(int labelIndex)
        {
            return FindLabel(labelIndex).vertices.Select(GetTextureColorAtVertex).ToList();
        }

        private Color32 GetTextureColorAtVertex(int vIndex)
        {
            Vector2 uv = Mesh.uv[vIndex];
            int x = Mathf.FloorToInt(uv.x * Texture.width);
            int y = Mathf.FloorToInt(uv.y * Texture.height);
            return Texture.GetPixel(x, y);
        }

        public Label FindLabel(int labelIndex)
        {
            return Labels.Find(l => l.index == labelIndex);
        }

        public void HideLabel(int labelIndex)
        {
            var label = FindLabel(labelIndex);
            label.color.a = 0;
            labelsChanged?.Invoke(LabelEvent.VisibleUpdate, new List<Label> { label });
            ShaderUpdater.UpdateLabelColor(label);
        }

        public void ShowLabel(int labelIndex)
        {
            var label = FindLabel(labelIndex);
            label.color.a = 1;
            labelsChanged?.Invoke(LabelEvent.VisibleUpdate, new List<Label> { label });
            ShaderUpdater.UpdateLabelColor(label);
        }

        public int GetFirstAvailableLabelIndex()
        {
            for (int i = 0; i < Label.Max; i++)
            {
                if (FindLabel(i) == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool LabelExists(int labelIndex)
        {
            return FindLabel(labelIndex) != null;
        }

        private void InitVertexColors()
        {
            var colors = new Color[Mesh.vertices.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(0, 0, 0, 0);
            }

            Mesh.colors = colors;
        }

        private void InitLabels()
        {
            var labels = LabelJsonPersister.Load().ToList();
            Labels = new List<Label>(labels);
            labelsChanged?.Invoke(LabelEvent.Add, labels);
        }

        public void RemoveLabel(Label label)
        {
            // Remove label from list
            Labels.Remove(label);

            // Remove label from shader
            ShaderUpdater.RemoveShaderLabel(label);

            // Remove label from disk
            LabelJsonPersister.Save(Labels);
            labelsChanged?.Invoke(LabelEvent.Remove, new List<Label> { label });
        }

        public void UpdateLabel(int labelIndex, string newName, string newDescription, Color newColor)
        {
            if (!LabelExists(labelIndex))
            {
                var errMessage = $"Label with index {labelIndex} doesn't exist!";
                Debug.LogError(errMessage);
                throw new Exception(errMessage);
            }

            int listIndex = Labels.FindIndex(l => l.index == labelIndex);
            Labels[listIndex].name = newName;
            Labels[listIndex].description = newDescription;
            Labels[listIndex].color = newColor;

            ShaderUpdater.UpdateLabelColor(Labels[listIndex]);

            LabelJsonPersister.Save(Labels);
            
            labelsChanged?.Invoke(LabelEvent.Update, new List<Label> { Labels[listIndex] });
        }

        public Label NewLabel()
        {
            var newLabel = new Label(GetFirstAvailableLabelIndex());

            var index = newLabel.index;
            if (LabelExists(index))
            {
                var errMessage = $"Label with index {index} already exists!";
                Debug.LogError(errMessage);
                throw new Exception(errMessage);
            }

            // Add label to list
            Labels.Add(newLabel);

            // Add color to shader
            ShaderUpdater.UpdateLabelColor(newLabel);

            // Add label to disk
            LabelJsonPersister.Save(Labels);
            
            labelsChanged?.Invoke(LabelEvent.Add, new List<Label> {newLabel});
            
            return newLabel;
        }
    }
}