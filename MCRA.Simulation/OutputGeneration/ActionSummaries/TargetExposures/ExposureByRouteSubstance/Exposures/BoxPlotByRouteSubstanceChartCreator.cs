using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BoxPlotByRouteSubstanceChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExposureByRouteSubstancePercentileRecord> _records;
        private readonly string _route;
        private readonly string _sectionId;
        private readonly TargetUnit _unit;
        private readonly bool _showOutLiers;

        public BoxPlotByRouteSubstanceChartCreator(
            List<ExposureByRouteSubstancePercentileRecord> records,
            string route,
            string sectionId,
            TargetUnit unit,
            bool showOutLiers
        ) {
            _records = records;
            _route = route;
            _sectionId = sectionId;
            _unit = unit;
            _showOutLiers = showOutLiers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override string ChartId {
            get {
                var pictureId = "c14c7c93-94e4-4e69-8c73-a91407d85d94";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _route);
            }
        }

        public override PlotModel Create() {
            return create(
                records: _records.Cast<BoxPlotChartRecord>().ToList(),
                labelHorizontalAxis: $"Exposure ({_unit.GetShortDisplayName()})",
                showOutLiers: _showOutLiers
            );
        }
    }
}
