using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicFoodAsEatenPieChartCreator : PieChartCreatorBase {

        private DietaryChronicDrillDownRecord _record;

        public DietaryChronicFoodAsEatenPieChartCreator(DietaryChronicDrillDownRecord record) {
            Width = 500;
            Height = 350;
            _record = record;
        }

        public override string Title => "Total exposure per body weight/day for foods as eaten";

        public override string ChartId {
            get {
                var pictureId = "ba31842f-065f-43b2-9c6a-e5720a4351f7";
                return StringExtensions.CreateFingerprint(_record.Guid + pictureId);
            }
        }

        public override PlotModel Create() {
           var records = _record.DayDrillDownRecords
                .SelectMany(dd => dd.IntakeSummaryPerFoodAsEatenRecords)
                .GroupBy(dd => dd.FoodName)
                .Select(g => (FoodName: g.Key, IntakePerBodyWeight: g.Sum(s => s.IntakePerMassUnit)))
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
            