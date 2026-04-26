using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
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

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
        }

        /// <summary>
        /// Scans the 3D mesh, finds all vertices painted by the mask, 
        /// and returns their multispectral signatures.
        /// </summary>
        public List<ushort[]> GetLabelSignatures(RenderTexture maskArray, int sliceIndex, int maskWidth, int maskHeight)
        {
            List<ushort[]> signatures = new List<ushort[]>();
            
            if (MSTex == null || maskArray == null) 
            {
                return signatures;
            }

            // 1. ALLOCATE: Create staging buffers to download the mask from the GPU
            RenderTexture stagingRT = new RenderTexture(maskWidth, maskHeight, 0, RenderTextureFormat.R8);
            Texture2D cpuMaskTex = TextureUtils.CreateRaw(maskWidth, maskHeight, TextureFormat.RGB24);

            // 2. DOWNLOAD: VRAM -> RAM
            Graphics.CopyTexture(maskArray, sliceIndex, 0, stagingRT, 0, 0);
            RenderTexture.active = stagingRT;
            cpuMaskTex.ReadPixels(new Rect(0, 0, maskWidth, maskHeight), 0, 0);
            cpuMaskTex.Apply();
            RenderTexture.active = null;

            // Extract the flat pixel array from RAM
            Color32[] maskPixels = cpuMaskTex.GetPixels32();

            // 3. ANALYZE: Loop through every physical corner of the 3D model
            for (int i = 0; i < Mesh.vertices.Length; i++)
            {
                // Find where this 3D point sits on the 2D texture
                int flatIndex = TextureUtils.ComputePixelIndex(cpuMaskTex, Mesh.uv[i]);
                
                // If this vertex's UV coordinate lands on a pixel we painted (Red > 0)
                if (flatIndex >= 0 && flatIndex < maskPixels.Length && maskPixels[flatIndex].r > 0)
                {
                    // Extract the multispectral signature for this point
                    signatures.Add(GetSignatureAtVertex(i));
                }
            }

            // 4. CLEANUP: Destroy the staging buffers so we don't leak memory!
            Destroy(stagingRT);
            Destroy(cpuMaskTex);

            return signatures;
        }

        private ushort[] GetSignatureAtVertex(int vIndex)
        {
            if (MSTex == null)
            {
                return Array.Empty<ushort>();
            }

            int flatIndex = TextureUtils.ComputePixelIndex(MSTex, Mesh.uv[vIndex]);
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