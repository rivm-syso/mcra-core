using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class GroupedScreeningPieChartCreator : ReportPieChartCreatorBase {

        private ScreeningSummarySection _section;

        public GroupedScreeningPieChartCreator(ScreeningSummarySection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }
        public override string Title => "Highest risk drivers";

        public override string ChartId {
            get {
                var pictureId = "266f7624-bbb6-4211-bf29-55cf4da9d1d0";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _section.GroupedScreeningSummaryRecords;
            var otherRecord = records.FirstOrDefault(c => c.CompoundCode == "Others");
            records = records.OrderByDescending(r => r.Contribution).ToList();
            var pieSlices = records.Select(c => new PieSlice($"{c.CompoundName}/{c.FoodAsMeasuredName} ({c.NumberOfFoods})", c.Contribution)).ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.PurpleToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}
