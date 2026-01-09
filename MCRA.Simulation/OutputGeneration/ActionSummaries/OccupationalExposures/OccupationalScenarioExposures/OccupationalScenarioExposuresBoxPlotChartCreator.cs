using ExCSS;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalScenarioExposuresBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<OccupationalScenarioExposureDistributionBoxPlotRecord> _records;
        private readonly string _route;
        private readonly string _sectionId;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public OccupationalScenarioExposuresBoxPlotChartCreator(
            List<OccupationalScenarioExposureDistributionBoxPlotRecord> records,
            string route,
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
                var pictureId = "0D3924FF-D6CC-4B16-831F-FD4D4258E044";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _route);
            }
        }

        public override PlotModel Create() {
            var result = create(
                _records.Cast<BoxPlotChartRecord>().ToList(),
                $"Exposure ({_unit})",
                _showOutliers,
                true
            );
            var rc = new PdfRenderContext(Width, Height, OxyColors.Transparent);
            var leftMargin = _records
                .Max(r => rc.MeasureText(r.GetLabel(), result.DefaultFont, result.DefaultFontSize, result.SubtitleFontWeight).Width);
            result.PlotMargins = new OxyThickness(leftMargin + 20, double.NaN, double.NaN, double.NaN);
            return result;
        }
    }
}
