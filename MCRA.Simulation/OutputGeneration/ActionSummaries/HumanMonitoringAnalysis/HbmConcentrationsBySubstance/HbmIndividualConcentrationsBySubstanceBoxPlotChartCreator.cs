using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator : HbmConcentrationsBoxPlotChartCreatorBase {

        private readonly HbmIndividualDistributionBySubstanceSection _section;
        private readonly string _unit;

        public HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator(HbmIndividualDistributionBySubstanceSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "f40513c6-7e3a-46cd-903f-d2930c2dc8da";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmBoxPlotRecords, $"Concentration ({_unit})");
        }
    }
}
