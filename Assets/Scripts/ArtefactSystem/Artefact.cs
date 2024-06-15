using System;
using System.Collections.Generic;
using System.Linq;
using LabelSystem;
using LabelSystem.Utils;
using UnityEngine;

namespace ArtefactSystem
{
    public class Artefact : MonoBehaviour
    {
        public Renderer Renderer { get; private set; }
        private MeshFilter MeshFilter { get; set; }
        public Mesh Mesh => MeshFilter.sharedMesh;

        public List<Label> Labels { get; private set; }

        public ShaderLabelUpdater ShaderUpdater { get; private set; }

        public event EventHandler<LabelsChangedEventArgs> LabelsChanged;

        public class LabelsChangedEventArgs
        {
            public LabelEvent Type;
            public List<Label> Items;
        }

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            ShaderUpdater = GetComponent<ShaderLabelUpdater>();
            InitVertexColors();
            InitLabels();
        }

        public Label FindLabel(int labelIndex)
        {
            return Labels.Find(l => l.index == labelIndex);
        }

        public void HideLabel(int labelIndex)
        {
            var label = FindLabel(labelIndex);
            label.color.a = 0;
            ShaderUpdater.UpdateLabelColor(label);
        }
        
        public void ShowLabel(int labelIndex)
        {
            var label = FindLabel(labelIndex);
            label.color.a = 1;
            ShaderUpdater.UpdateLabelColor(label);
        }

        public void HideAllLabels()
        {
            Labels.ForEach(label =>
            {
                label.color.a = 0;
            });
            ShaderUpdater.UpdateLabelColors(Labels);
        }

        public void ShowAllLabels()
        {
            Labels.ForEach(label =>
            {
                label.color.a = 1;
            });
            ShaderUpdater.UpdateLabelColors(Labels);
        }
        
        private void Update()
        {
            return;
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
            var labels = LabelJsonUtil.Load().ToList();
            Labels = new List<Label>(labels);
            
            LabelsChanged?.Invoke(this, new LabelsChangedEventArgs { Type = LabelEvent.Add, Items = labels });
        }

        public void RemoveLabel(Label label)
        {
            // Remove label from list
            Labels.Remove(label);

            // Remove label from shader
            ShaderUpdater.RemoveShaderLabel(label);

            // Remove label from disk
            LabelJsonUtil.Save(Labels);
            
            LabelsChanged?.Invoke(this,
                new LabelsChangedEventArgs { Type = LabelEvent.Remove, Items = new List<Label> { label } });
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
            
            LabelJsonUtil.Save(Labels);

            LabelsChanged?.Invoke(this,
                new LabelsChangedEventArgs { Type = LabelEvent.Update, Items = new List<Label> { Labels[listIndex] } });
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

            // Add label to disk
            LabelJsonUtil.Save(Labels);
            
            LabelsChanged?.Invoke(this,
                new LabelsChangedEventArgs { Type = LabelEvent.Add, Items = new List<Label> { newLabel } });
            
            return newLabel;
        }
    }
}