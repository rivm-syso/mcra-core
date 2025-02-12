using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BoxPlotByRouteChartCreator : BoxPlotByRouteChartCreatorBase {

        private readonly List<PercentilesRecordBase> _records;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public BoxPlotByRouteChartCreator(
            List<PercentilesRecordBase> records,
            string unit,
            bool showOutliers
        ) {
            _records = records;
            _unit = unit;
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "1c7a3cb3-c55b-41f9-9ba9-151571d3ab84";
                return StringExtensions.CreateFingerprint(pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_records, $"Exposure ({_unit})", _showOutliers);
        }
    }
}
