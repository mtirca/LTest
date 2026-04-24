using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Utils;
using Tools;

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

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
        }

        /// <summary>
        /// Scans the 3D mesh, finds all vertices painted by the label, 
        /// and returns their multispectral signatures.
        /// </summary>
        public List<ushort[]> GetLabelSignatures(Label label)
        {
            List<ushort[]> signatures = new List<ushort[]>();
            
            if (MSTex == null || label == null || label.Pixels == null) 
            {
                return signatures;
            }

            // Loop through every physical corner of the 3D model
            for (int i = 0; i < Mesh.vertices.Length; i++)
            {
                // Find where this 3D point sits on the 2D texture
                int flatIndex = TextureHelper.ComputePixelIndex(MSTex, Mesh.uv[i]);
                
                // If this vertex's UV coordinate lands on a pixel we painted (Red > 0)
                if (flatIndex >= 0 && flatIndex < label.Pixels.Length && label.Pixels[flatIndex].r > 0)
                {
                    // Extract the multispectral signature for this point
                    signatures.Add(GetSignatureAtVertex(i));
                }
            }

            return signatures;
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

            // Loop through the multispectral array and grab the data at this pixel
            for (int slice = 0; slice < bands; slice++)
            {
                NativeArray<ushort> rawSliceData = MSTex.GetPixelData<ushort>(0, slice);
                signature[slice] = rawSliceData[flatIndex];
            }

            return signature;
        }
    }
}