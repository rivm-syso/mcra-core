using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.ConsumerProductExposures;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposuresByRouteBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ConsumerProductExposuresPercentilesRecord> _records;
        private readonly ExposureRoute _route;
        private readonly string _sectionId;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public ConsumerProductExposuresByRouteBoxPlotChartCreator(
            List<ConsumerProductExposuresPercentilesRecord> records,
            ExposureRoute route,
            string sectionId,
            string unit,
            bool showOutliers
        ) {
            _records = records;
            _route = route;
            _sectionId = sectionId;
            _unit = unit;
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "cc2b0f52-4d54-4047-9a94-040ba12d860f";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _route.GetHashCode());
            }
        }

        public override PlotModel Create() {
            return create(
                _records.Cast<BoxPlotChartRecord>().ToList(),
                $"Exposure ({_unit})",
                _showOutliers,
                true
            );
        }
    }
}
