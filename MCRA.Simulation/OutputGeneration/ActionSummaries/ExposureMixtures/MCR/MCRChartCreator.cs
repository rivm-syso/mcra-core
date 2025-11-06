using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MCRChartCreator : DriverCompoundsChartCreatorBase {

        private readonly RiskMaximumCumulativeRatioSection _section;
        private readonly string _xTitle;

        public MCRChartCreator(RiskMaximumCumulativeRatioSection section) {
            Height = 400;
            Width = 500;
            _section = section;
            var riskMetricType = _section.RiskMetricType == RiskMetricType.ExposureHazardRatio ? "E/H" : "H/E";
            _xTitle = $"Risk characterisation ratio ({riskMetricType})";
        }

        public override string ChartId {
            get {
                var pictureId = "c80a9f6b-97d8-48f2-b09a-9de4c1a441e1";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Using MCR to identify substances that drive cumulative risk, scatter distributions (total).";

        public override PlotModel Create() {
            var plotModel = createMCRChart(
                _section.DriverSubstanceTargets,
                _section.RiskMetricType,
                _section.Threshold,
                _xTitle,
                renderLegend: false
            );
            return plotModel;
        }
    }
}
