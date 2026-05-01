using UnityEngine;
using XCharts.Runtime;

namespace UI
{
    [RequireComponent(typeof(LineChart))]
    public class ChartPlotter : MonoBehaviour
    {
        private LineChart _spectralChart;
        private XAxis _cachedXAxis;

        private void Awake()
        {
            _spectralChart = GetComponent<LineChart>();
        }

        /// <summary>
        /// Initializes the axes and chart visuals. Call this once during setup.
        /// </summary>
        public void InitializeChart()
        {
            _spectralChart.RemoveData();
            _spectralChart.EnsureChartComponent<Title>().show = false;
            _spectralChart.EnsureChartComponent<Tooltip>().show = true;

            _cachedXAxis = _spectralChart.EnsureChartComponent<XAxis>();
            _cachedXAxis.axisName.name = "Wavelength (nm)";
            _cachedXAxis.axisName.show = true;
            _cachedXAxis.type = Axis.AxisType.Category;
            //todo doesnt work
            _cachedXAxis.axisLabel.showStartLabel = true;
            _cachedXAxis.axisLabel.showEndLabel = true;

            var yAxis = _spectralChart.EnsureChartComponent<YAxis>();
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
        /// Clears the chart and plots a single solid line.
        /// </summary>
        public void PlotSingleCurve(string serieName, float[] percentages, int[] wavelengths, Color lineColor)
        {
            _spectralChart.RemoveData();
            
            var serie = _spectralChart.AddSerie<Line>(serieName);
            serie.symbol.show = true;
            serie.symbol.size = 3;
            serie.lineStyle.width = 3;
            
            // XCharts expects a standard Unity Color
            serie.itemStyle.color = lineColor; 

            _cachedXAxis.data.Clear();

            for (int i = 0; i < percentages.Length; i++)
            {
                _spectralChart.AddData(0, i, percentages[i]);

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
    }
}
