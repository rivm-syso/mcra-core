using CommandLine;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRouteSubstance {
    public class ExternalExposuresByRouteSubstanceBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExternalExposuresPercentilesRecord> _records;
        private readonly ExposureRoute _exposureRoute;
        private readonly ExternalExposuresByRouteSubstanceSection _section;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public ExternalExposuresByRouteSubstanceBoxPlotChartCreator(
            ExternalExposuresByRouteSubstanceSection section,
            List<ExternalExposuresPercentilesRecord> records,
            ExposureRoute route,
            string unit,
            bool showOutliers
        ) {
            _section = section;
            _records = records;
            _exposureRoute = route;
            _unit = unit;
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                return StringExtensions.CreateFingerprint(_section.SectionId + _section.PictureId + _exposureRoute.GetHashCode());
            }
        }

        public override PlotModel Create() {
            return create(
                [.. _records.Cast<BoxPlotChartRecord>()],
                $"Exposure ({_unit})",
                _showOutliers,
                true
            );
        }
    }
}
