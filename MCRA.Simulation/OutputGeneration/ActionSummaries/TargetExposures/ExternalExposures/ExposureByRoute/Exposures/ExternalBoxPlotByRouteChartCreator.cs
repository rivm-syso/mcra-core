using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExternalBoxPlotByRouteChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExternalExposureByRoutePercentileRecord> _records;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public ExternalBoxPlotByRouteChartCreator(
            List<ExternalExposureByRoutePercentileRecord> records,
            ExposureUnitTriple unit,
            bool showOutliers
        ) {
            _records = records;
            _unit = unit.GetShortDisplayName();
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override string ChartId {
            get {
                var pictureId = "749cc839-dec8-4b9b-84a1-4a44db48e8c2";
                return StringExtensions.CreateFingerprint(pictureId);
            }
        }

        public override PlotModel Create() {
            return create(
                records: _records.Cast<BoxPlotChartRecord>().ToList(),
                labelHorizontalAxis: $"Exposure ({_unit})",
                showOutliers: _showOutliers
            );
        }
    }
}
