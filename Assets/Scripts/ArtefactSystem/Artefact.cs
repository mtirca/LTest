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
        public MeshCollider MeshCollider { get; set; }

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
            MeshCollider = GetComponent<MeshCollider>();
            ShaderUpdater = GetComponent<ShaderLabelUpdater>();
            InitVertexColors();
            InitLabels();
        }

        private void Update()
        {
            return;
        }

        public int GetFirstAvailableLabelIndex()
        {
            for (int i = 0; i < Label.Max; i++)
            {
                if (Labels.Find(label => label.index == i) == null)
                {
                    return i;
                }
            }

            return -1;
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
            LabelsChanged?.Invoke(this, new LabelsChangedEventArgs { Type = LabelEvent.Add, Items = labels });
            Labels = new List<Label>(labels);
        }

        /**
         * Adds a Label:
         * 1. Updates artefact's in memory labels
         * 2. Updates shader texture
         * 3. Updates on disk
         */
        public void AddLabel(Label label)
        {
            LabelsChanged?.Invoke(this,
                new LabelsChangedEventArgs { Type = LabelEvent.Add, Items = new List<Label> { label } });

            // Add label to list
            Labels.Add(label);

            // Update shader
            ShaderUpdater.AddLabelsToShader(new List<Label> { label });

            // Add label to disk
            LabelJsonUtil.Save(Labels);
        }

        public void RemoveLabel(Label label)
        {
            LabelsChanged?.Invoke(this,
                new LabelsChangedEventArgs { Type = LabelEvent.Remove, Items = new List<Label> { label } });

            // Remove label from list
            Labels.Remove(label);

            // Update shader
            ShaderUpdater.RemoveLabelsFromShader(new List<Label> { label });

            // Remove label from disk
            LabelJsonUtil.Save(Labels);
        }
    }
}