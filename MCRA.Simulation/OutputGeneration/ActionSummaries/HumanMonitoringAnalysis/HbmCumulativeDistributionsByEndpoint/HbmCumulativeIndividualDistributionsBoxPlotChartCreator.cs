using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsBoxPlotChartCreator : HbmConcentrationsBoxPlotChartCreatorBase {

        private readonly HbmCumulativeIndividualDistributionsSection _section;

        public HbmCumulativeIndividualDistributionsBoxPlotChartCreator(HbmCumulativeIndividualDistributionsSection section) {
            _section = section;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
        }

        public override string Title => "Cumulative HBM individual concentrations. " + base.Title;

        public override string ChartId {
            get {
                var pictureId = "f1145c79-846a-4077-a282-7d4c552745f7";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var unit = _section.HbmBoxPlotRecords.FirstOrDefault()?.Unit;
            return create(_section.HbmBoxPlotRecords, $"Cumulative concentration ({unit})", false);
        }
    }
}


