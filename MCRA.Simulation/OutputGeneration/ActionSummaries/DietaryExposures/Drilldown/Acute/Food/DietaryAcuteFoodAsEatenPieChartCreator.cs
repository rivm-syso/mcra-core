using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteFoodAsEatenPieChartCreator : ReportPieChartCreatorBase {

        private List<IndividualFoodDrillDownRecord> _records;
        private readonly int _ix;

        public DietaryAcuteFoodAsEatenPieChartCreator(List<IndividualFoodDrillDownRecord> records, int ix) {
            Width = 500;
            Height = 350;
            _records = records;
            _ix = ix;
        }

        //public override string Title => "Total exposure per body weight/day for foods as eaten";


        public override string ChartId {
            get {
                var pictureId = "11390b26-5d5a-4320-a5f2-be85633b1dbf";
                return StringExtensions.CreateFingerprint(_ix + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _records.Where(c => c.Exposure > 0).OrderByDescending(r => r.Exposure).ToList();
            var pieSlices = records.Select(c => new PieSlice(c.FoodName, c.Exposure)).ToList();
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
