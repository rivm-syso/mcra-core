using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteFoodAsMeasuredPieChartCreator : PieChartCreatorBase {

        private DietaryAcuteDrillDownRecord _record;

        public DietaryAcuteFoodAsMeasuredPieChartCreator(DietaryAcuteDrillDownRecord record) {
            Width = 500;
            Height = 350;
            _record = record;
        }
        public override string Title => "Total exposure per body weight/day for modelled foods";

        public override string ChartId {
            get {
                var pictureId = "0985c652-973b-41db-9879-3e23956a50c4";
                return StringExtensions.CreateFingerprint(_record.Guid + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _record.IntakeSummaryPerFoodAsMeasuredRecords.Where(c => c.Concentration > 0).OrderByDescending(r => r.IntakePerMassUnit).ToList();
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
