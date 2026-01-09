using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalExposuresByRouteBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<OccupationalExposuresPercentilesRecord> _records;
        private readonly ExposureRoute _route;
        private readonly string _sectionId;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public OccupationalExposuresByRouteBoxPlotChartCreator(
            List<OccupationalExposuresPercentilesRecord> records,
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
                var pictureId = "30112515-0d56-402f-8c56-cd4f12b81d98";
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
