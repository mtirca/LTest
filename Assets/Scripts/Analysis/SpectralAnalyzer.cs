using UnityEngine;
using System.Collections.Generic;

namespace Analysis
{
    public class SpectralAnalyzer : MonoBehaviour
    {
        private static readonly int MaskTex = Shader.PropertyToID("MaskTex");
        private static readonly int MSTex = Shader.PropertyToID("MSTex");
        private static readonly int OutputSignatures = Shader.PropertyToID("OutputSignatures");
        private static readonly int ResultCounter = Shader.PropertyToID("ResultCounter");
        private static readonly int SliceIndex = Shader.PropertyToID("_SliceIndex");
        private static readonly int TotalBands = Shader.PropertyToID("_TotalBands");
        private static readonly int MaxExpectedPixels = Shader.PropertyToID("_MaxExpectedPixels");

        [Header("GPU Compute")]
        [SerializeField] private ComputeShader spectralAverager;

        [Header("Config")] [SerializeField]
        private int maxExpectedPixels = 500_000; 

        public List<ushort[]> ExtractSignatures(RenderTexture maskTex, Texture2DArray msTex, int sliceIndex)
        {
            int totalBands = msTex.depth;
            List<ushort[]> signatures = new List<ushort[]>();

            if (maskTex == null || msTex == null || spectralAverager == null) 
            {
                Debug.LogError("SpectralAnalyzer is missing references!");
                return signatures;
            }

            int width = maskTex.width;
            int height = maskTex.height;
            
            ComputeBuffer counterBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            counterBuffer.SetData(new[] { 0 }); 

            ComputeBuffer outputBuffer = new ComputeBuffer(maxExpectedPixels * totalBands, sizeof(float));

            int kernel = spectralAverager.FindKernel("SpectralAverager");
            spectralAverager.SetTexture(kernel, MaskTex, maskTex);
            spectralAverager.SetTexture(kernel, MSTex, msTex);
            spectralAverager.SetBuffer(kernel, OutputSignatures, outputBuffer);
            spectralAverager.SetBuffer(kernel, ResultCounter, counterBuffer);
            
            spectralAverager.SetInt(SliceIndex, sliceIndex);
            spectralAverager.SetInt(TotalBands, totalBands);
            spectralAverager.SetInt(MaxExpectedPixels, maxExpectedPixels);

            int threadGroupsX = Mathf.CeilToInt(width / 8f);
            int threadGroupsY = Mathf.CeilToInt(height / 8f);
            spectralAverager.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

            int[] countResult = new int[1];
            counterBuffer.GetData(countResult);
            
            int foundPixels = Mathf.Min(countResult[0], maxExpectedPixels);

            if (foundPixels > 0)
            {
                float[] flatSignatures = new float[foundPixels * totalBands];
                outputBuffer.GetData(flatSignatures);

                // UPDATED: Convert the 0.0-1.0 floats back to 0-65535 ushorts
                for (int p = 0; p < foundPixels; p++)
                {
                    ushort[] singlePixelSignature = new ushort[totalBands];
                    for (int b = 0; b < totalBands; b++)
                    {
                        // Extract the raw float
                        float rawFloat = flatSignatures[p * totalBands + b];
                        
                        // Multiply by 65535, round to nearest whole number, and clamp just to be perfectly safe
                        singlePixelSignature[b] = (ushort)Mathf.Clamp(Mathf.RoundToInt(rawFloat * 65535f), 0, 65535);
                    }
                    signatures.Add(singlePixelSignature);
                }
            }
            else
            {
                Debug.LogWarning($"Analyzer found 0 painted pixels for slice {sliceIndex}");
            }

            counterBuffer.Release();
            outputBuffer.Release();

            return signatures;
        }
    }
}