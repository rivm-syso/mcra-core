using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SoilExposures;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SoilExposuresByRouteBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<SoilExposuresPercentilesRecord> _records;
        private readonly ExposureRoute _route;
        private readonly string _sectionId;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public SoilExposuresByRouteBoxPlotChartCreator(
            List<SoilExposuresPercentilesRecord> records,
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

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override string ChartId {
            get {
                var pictureId = "68f6d4e5-6076-4d3d-9b55-8d5b67d38fd0";
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
