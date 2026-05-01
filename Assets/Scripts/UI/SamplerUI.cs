using UnityEngine;
using XCharts.Runtime;

namespace UI
{
    public class SamplerUI : MonoBehaviour
    {
        [SerializeField] private ChartPlotter plotter;

        private XAxis _cachedXAxis;

        private void Start()
        {
            plotter.InitializeChart();
        }

        /// <summary>
        /// Accepts raw 16-bit sensor data (0-65535), converts it to percentages, 
        /// and sends it to the plotting engine.
        /// </summary>
        /// <param name="rawValues">The raw values for a pixel, one for each band.</param>
        /// <param name="wavelengths">The wavelength values, one for each band.</param>
        /// todo add an enum SetType.RELATIVE_INTENSITY or something. if i have white reference SetType.WHITE_REFERENCE
        ///  or sth idkkkkkkkkkkk check later if needed
        public void SetRawData(ushort[] rawValues, int[] wavelengths = null)
        {
            float[] floatValues = new float[rawValues.Length];

            for (int i = 0; i < rawValues.Length; i++)
            {
                float percentage = rawValues[i] / 65535f * 100f;
                floatValues[i] = percentage;
            }

            // Using Cyan as a default "Sampling" color
            plotter.PlotSingleCurve("Sample Point", floatValues, wavelengths, Color.cyan);
        }
    }
}