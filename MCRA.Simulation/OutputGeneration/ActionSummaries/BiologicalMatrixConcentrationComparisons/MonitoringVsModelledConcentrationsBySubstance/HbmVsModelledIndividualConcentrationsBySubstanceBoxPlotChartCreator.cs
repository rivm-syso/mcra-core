using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmVsModelledIndividualConcentrationsBySubstanceBoxPlotChartCreator : HbmVsModelledBoxPlotChartCreatorBase {

        private HbmVsModelledIndividualConcentrationsBySubstanceSection _section;
        private readonly string _unit;

        public HbmVsModelledIndividualConcentrationsBySubstanceBoxPlotChartCreator(HbmVsModelledIndividualConcentrationsBySubstanceSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "d34134b4-7e4a-49ea-9e75-5f8c064422a2";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95; monitoring (blue) vs modelled (green)";

        public override PlotModel Create() {
            return create(_section.HbmBoxPlotRecords, $"Concentration ({_unit})");
        }
    }
}
