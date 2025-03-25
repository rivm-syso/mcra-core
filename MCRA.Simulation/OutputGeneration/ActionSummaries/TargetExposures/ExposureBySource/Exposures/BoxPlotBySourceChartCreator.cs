using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BoxPlotBySourceChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExposureBySourcePercentileRecord> _records;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public BoxPlotBySourceChartCreator(
            List<ExposureBySourcePercentileRecord> records,
            TargetUnit unit,
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
                var pictureId = "4b07de48-d2cc-437d-a275-2a8dfe7a172e";
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
