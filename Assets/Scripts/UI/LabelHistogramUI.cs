using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace UI
{
    public class LabelHistogramUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("The component responsible for rendering the graph")]
        [SerializeField] private ChartPlotter plotter;

        private void Start()
        {
            plotter.InitializeChart();
        }

        /// <summary>
        /// Calculates the mean average of a label's pixels and plots it.
        /// </summary>
        public void PlotLabelAverage(Label label, List<ushort[]> pixelSignatures, int[] wavelengths)
        {
            if (plotter == null) return;

            if (pixelSignatures == null || pixelSignatures.Count == 0)
            {
                Debug.LogWarning($"HistogramUI: No pixels found for label {label.name}");
                return;
            }

            int bands = pixelSignatures[0].Length;
            int pixelCount = pixelSignatures.Count;
    
            long[] sums = new long[bands];

            foreach (ushort[] signature in pixelSignatures)
            {
                for (int b = 0; b < bands; b++)
                {
                    sums[b] += signature[b];
                }
            }

            float[] meanPercentages = new float[bands];
            for (int b = 0; b < bands; b++)
            {
                float meanRaw = (float)sums[b] / pixelCount;
                meanPercentages[b] = meanRaw / 65535f * 100f;
            }

            plotter.PlotSingleCurve(label.name, meanPercentages, wavelengths, label.color);
        }
    }
}