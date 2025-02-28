using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicModelledFoodPieChartCreator : ReportPieChartCreatorBase {

        private List<IndividualFoodDrillDownRecord> _records;
        private readonly int _ix;

        public DietaryChronicModelledFoodPieChartCreator(List<IndividualFoodDrillDownRecord> records, int ix) {
            Width = 500;
            Height = 350;
            _records = records;
            _ix = ix;
        }


        public override string Title => "Total exposure per body weight/day for modelled foods";

        public override string ChartId {
            get {
                var pictureId = "21d41fe4-757a-44ea-b286-a4a28bc1bb79";
                return StringExtensions.CreateFingerprint(_ix + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _records.GroupBy(dd => dd.FoodName)
                .Select(g => (
                    FoodName: g.Key,
                    IntakePerBodyWeight: g.Sum(s => s.Exposure))
                )
                .OrderByDescending(c => c.IntakePerBodyWeight)
                .ToList();
            var pieSlices = records.Select(c => new PieSlice(c.FoodName, c.IntakePerBodyWeight)).ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GreenToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}
