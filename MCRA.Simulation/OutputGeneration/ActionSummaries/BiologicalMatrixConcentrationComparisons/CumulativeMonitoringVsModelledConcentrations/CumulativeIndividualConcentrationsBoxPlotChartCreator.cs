using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeIndividualConcentrationsBoxPlotChartCreator : HbmVsModelledBoxPlotChartCreatorBase {

        private CumulativeIndividualConcentrationsSection _section;
        private readonly string _unit;

        public CumulativeIndividualConcentrationsBoxPlotChartCreator(CumulativeIndividualConcentrationsSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.Records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "77b29c7f-e822-4d9d-9716-297591ef9743";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95; monitoring (blue) vs modelled (green)";

        public override PlotModel Create() {
            return create(_section.Records, _unit, true);
        }
    }
}
