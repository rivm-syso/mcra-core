using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicFoodAsEatenPieChartCreator : ReportPieChartCreatorBase {

        private readonly List<IndividualFoodDrillDownRecord> _records;
        private readonly int _ix;
        public DietaryChronicFoodAsEatenPieChartCreator(List<IndividualFoodDrillDownRecord> records, int ix) {
            Width = 500;
            Height = 350;
            _records = records;
            _ix = ix;
        }

        public override string Title => "Total exposure per body weight/day for foods as eaten";

        public override string ChartId {
            get {
                var pictureId = "ba31842f-065f-43b2-9c6a-e5720a4351f7";
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
            var plotModel = base.create(pieSlices, noSlices);
            return plotModel;
        }
    }
}
