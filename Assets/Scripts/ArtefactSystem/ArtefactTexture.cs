using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Exceptions;
using UnityEngine;

namespace ArtefactSystem
{
    public class ArtefactTexture : MonoBehaviour
    {
        [SerializeField] private Artefact artefact;

        private static readonly int RedBandID = Shader.PropertyToID("_RedBand");
        private static readonly int GreenBandID = Shader.PropertyToID("_GreenBand");
        private static readonly int BlueBandID = Shader.PropertyToID("_BlueBand");
        private static readonly int IsCompositeID = Shader.PropertyToID("_IsComposite");

        private void Awake()
        {
            ResetToDefaultView();
        }

        /// <summary>
        /// Returns the shader to standard single-band grayscale viewing.
        /// </summary>
        public void ResetToDefaultView()
        {
            if (artefact.Renderer != null && artefact.Renderer.material != null)
            {
                artefact.Renderer.material.SetFloat(IsCompositeID, 0f);
            }
        }

        /// <summary>
        /// Maps three specific multispectral slices to the visual R, G, and B channels on the GPU.
        /// </summary>
        /// <param name="redSlice">The index of the slice of the multispectral texture to be mapped to the screen's Red channel.</param>
        /// <param name="greenSlice">The index of the slice of the multispectral texture to be mapped to the screen's Green channel.</param>
        /// <param name="blueSlice">The index of the slice of the multispectral texture to be mapped to the screen's Blue channel.</param>
        public void SetCompositeTexture(int redSlice, int greenSlice, int blueSlice)
        {
            ValidateSliceBounds(redSlice, greenSlice, blueSlice);

            var mat = artefact.Renderer.material;

            mat.SetFloat(RedBandID, redSlice);
            mat.SetFloat(GreenBandID, greenSlice);
            mat.SetFloat(BlueBandID, blueSlice);

            mat.SetFloat(IsCompositeID, 1f);
        }

        private void ValidateSliceBounds(int r, int g, int b)
        {
            var invalidChannels = new List<string>();

            int bands = artefact.MSTex.depth;
            if (r < 0 || r > bands - 1)
            {
                invalidChannels.Add($"Red ({r})");
            }

            if (g < 0 || g > bands - 1)
            {
                invalidChannels.Add($"Green ({g})");
            }

            if (b < 0 || b > bands - 1)
            {
                invalidChannels.Add($"Blue ({b})");
            }

            if (invalidChannels.Count > 0)
            {
                string errMessage =
                    $"Slice indices out of bounds. Maximum value is {bands - 1}. Invalid band values: {string.Join(", ", invalidChannels)}";
                Debug.LogError(errMessage);
                throw new ArgumentOutOfRangeException(errMessage);
            }
        }
    }
}