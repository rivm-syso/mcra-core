using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BoxPlotBySourceRouteChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExposureBySourceRoutePercentileRecord> _records;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public BoxPlotBySourceRouteChartCreator(
            List<ExposureBySourceRoutePercentileRecord> records,
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
                var pictureId = "3f495dfe-9073-4ee3-9d79-f9913eb99284";
                return StringExtensions.CreateFingerprint(pictureId);
            }
        }

        public override PlotModel Create() {
            return create(
                records: _records.Cast<BoxPlotChartRecord>().ToList(),
                labelHorizontalAxis: $"Exposure ({_unit})",
                showOutLiers: _showOutliers
            );
        }
    }
}
