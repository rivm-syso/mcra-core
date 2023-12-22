using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ScreeningPieChartCreator : ReportPieChartCreatorBase {

        private ScreeningSummarySection _section;

        public ScreeningPieChartCreator(ScreeningSummarySection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string Title => "Highest risk driver components";

        public override string ChartId {
            get {
                var pictureId = "de2711e1-9917-436d-b2d0-3af6cdd375fd";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _section.ScreeningSummaryRecords.OrderByDescending(r => r.CupPercentage).ToList();
            var pieSlices = records.Select(c => new PieSlice(label: $"{c.CompoundName}/{c.FoodAsMeasuredName}/{c.FoodAsEatenName}", c.CupPercentage)).ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getPieSplit(pieSlices, 0.99, 10);
            var palette = CustomPalettes.PurpleToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}
