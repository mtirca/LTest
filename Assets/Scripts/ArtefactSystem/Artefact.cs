using System;
using System.Collections.Generic;
using System.Linq;
using LabelSystem;
using LabelSystem.JsonPersister;
using Tools;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace ArtefactSystem
{
    public class Artefact : MonoBehaviour
    {
        public Renderer Renderer { get; private set; }
        private MeshFilter MeshFilter { get; set; }
        public Mesh Mesh => MeshFilter.sharedMesh;
        public MeshCollider MeshCollider { get; private set; }
        [Tooltip("The specific wavelengths (in nm) for each slice of the texture")]
        public int[] Wavelengths;
        
        private static readonly int MSTexID = Shader.PropertyToID("_MSTex");
        public Texture2DArray MSTex
        {
            //todo
            get => Renderer.material.GetTexture(MSTexID) as Texture2DArray;
            set => Renderer.material.SetTexture(MSTexID, value);
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

        //todo update usages
        public List<ushort[]> GetLabelVerticesSignatures(int labelIndex)
        {
            return FindLabel(labelIndex).vertices.Select(GetSignatureAtVertex).ToList();
        }

        private ushort[] GetSignatureAtVertex(int vIndex)
        {
            if (MSTex == null)
            {
                return Array.Empty<ushort>();
            }

            int flatIndex = TextureHelper.ComputePixelIndex(MSTex, Mesh.uv[vIndex]);
            int bands = MSTex.depth;
            ushort[] signature = new ushort[bands];

            for (int slice = 0; slice < bands; slice++)
            {
                NativeArray<ushort> rawSliceData = MSTex.GetPixelData<ushort>(0, slice);
                signature[slice] = rawSliceData[flatIndex];
            }

            return signature;
        }

        public Label FindLabel(int labelIndex)
        {
            return Labels.Find(l => l.id == labelIndex);
        }

        /**
         * Hide by updating label in state, UI and shader
         */
        public void HideLabel(int labelIndex)
        {
            var label = FindLabel(labelIndex);
            label.Hide();
            labelsChanged?.Invoke(LabelEvent.VisibleUpdate, new List<Label> { label });
            ShaderUpdater.UpdateLabelColor(label);
        }

        /**
         * Show by updating label in state, UI and shader
         */
        public void ShowLabel(int labelIndex)
        {
            var label = FindLabel(labelIndex);
            label.Show();
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
            labels.ForEach(label => label.color.a = 0);
            Labels = new List<Label>(labels);
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

            int listIndex = Labels.FindIndex(l => l.id == labelIndex);
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

            var index = newLabel.id;
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

            labelsChanged?.Invoke(LabelEvent.Add, new List<Label> { newLabel });

            return newLabel;
        }
    }
}