using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDayDistributionsBoxPlotChartCreator : HbmConcentrationsBoxPlotChartCreatorBase {

        private readonly HbmCumulativeIndividualDayDistributionsSection _section;

        public HbmCumulativeIndividualDayDistributionsBoxPlotChartCreator(HbmCumulativeIndividualDayDistributionsSection section) {
            _section = section;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
        }

        public override string Title => "Cumulative HBM individual day concentrations. " + base.Title;

        public override string ChartId {
            get {
                var pictureId = "b387b501-8483-4e5f-b9ec-61543576b0a5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var unit = _section.HbmBoxPlotRecords.FirstOrDefault()?.Unit;
            return create(_section.HbmBoxPlotRecords, $"Cumulative concentration ({unit})", false);
        }
    }
}
