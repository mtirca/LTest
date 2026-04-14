using UnityEngine;
using XCharts.Runtime;

namespace UI
{
    public class SamplerUI : MonoBehaviour
    {
        [Header("Chart References")] [SerializeField]
        private LineChart spectralChart;

        private XAxis _cachedXAxis;

        private void Start()
        {
            spectralChart.RemoveData();
            spectralChart.EnsureChartComponent<Title>().show = false;
            spectralChart.EnsureChartComponent<Tooltip>().show = true;

            _cachedXAxis = spectralChart.EnsureChartComponent<XAxis>();
            _cachedXAxis.axisName.name = "Wavelength (nm)";
            _cachedXAxis.axisName.show = true;
            _cachedXAxis.type = Axis.AxisType.Category;
            //todo doesnt work
            _cachedXAxis.axisLabel.showStartLabel = true;
            _cachedXAxis.axisLabel.showEndLabel = true;

            var yAxis = spectralChart.EnsureChartComponent<YAxis>();
            yAxis.axisName.name = "Relative Intensity (%)";
            yAxis.axisName.show = true;
            yAxis.type = Axis.AxisType.Value;
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
            yAxis.min = 0;
            yAxis.max = 100;
            yAxis.axisLabel.show = true;
            yAxis.axisLabel.formatter = "{value}%";
        }

        /// <summary>
        /// Plots the data.
        /// </summary>
        /// <param name="bandValues">The values of the pixel to plot, one for each band.</param>
        /// <param name="wavelengths">The wavelengths of the bands.</param>
        public void SetData(float[] bandValues, int[] wavelengths = null)
        {
            spectralChart.ClearData();
            spectralChart.AddSerie<Line>("Spectral Curve");

            _cachedXAxis.data.Clear();

            for (int i = 0; i < bandValues.Length; i++)
            {
                spectralChart.AddData(0, i, bandValues[i]);

                if (wavelengths != null && i < wavelengths.Length)
                {
                    _cachedXAxis.data.Add(wavelengths[i].ToString());
                }
                else
                {
                    _cachedXAxis.data.Add($"Band {i}");
                }
            }
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

            SetData(floatValues, wavelengths);
        }
    }
}