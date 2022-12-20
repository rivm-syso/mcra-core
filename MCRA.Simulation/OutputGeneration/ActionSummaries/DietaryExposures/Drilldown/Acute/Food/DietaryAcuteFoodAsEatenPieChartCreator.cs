using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteFoodAsEatenPieChartCreator : PieChartCreatorBase {

        private DietaryAcuteDrillDownRecord _record;

        public DietaryAcuteFoodAsEatenPieChartCreator(DietaryAcuteDrillDownRecord record) {
            Width = 500;
            Height = 350;
            _record = record;
        }

        public override string Title => "Total exposure per body weight/day for foods as eaten";


        public override string ChartId {
            get {
                var pictureId = "11390b26-5d5a-4320-a5f2-be85633b1dbf";
                return StringExtensions.CreateFingerprint(_record.Guid + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _record.IntakeSummaryPerFoodAsEatenRecords.Where(c => c.Concentration > 0).OrderByDescending(c => c.IntakePerMassUnit).ToList();
            var pieSlices = records.Select(c => new PieSlice(c.FoodName, c.IntakePerMassUnit)).ToList();
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
