using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicFoodAsMeasuredPieChartCreator : PieChartCreatorBase {

        private DietaryChronicDrillDownRecord _record;

        public DietaryChronicFoodAsMeasuredPieChartCreator(DietaryChronicDrillDownRecord record) {
            Width = 500;
            Height = 350;
            _record = record;
        }


        public override string Title => "Total exposure per body weight/day for modelled foods";

        public override string ChartId {
            get {
                var pictureId = "21d41fe4-757a-44ea-b286-a4a28bc1bb79";
                return StringExtensions.CreateFingerprint(_record.Guid + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _record.DayDrillDownRecords
                .SelectMany(dd => dd.IntakeSummaryPerFoodAsMeasuredRecords)
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
            var palette = CustomPalettes.GreenToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}
