using System.Collections.Generic;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteFoodAsMeasuredPieChartCreator : ReportPieChartCreatorBase {

        private readonly List<IndividualFoodDrillDownRecord> _records;
        private readonly int _ix;

        public DietaryAcuteFoodAsMeasuredPieChartCreator(List<IndividualFoodDrillDownRecord> records, int ix) {
            Width = 500;
            Height = 350;
            _records = records;
            _ix = ix;
        }
        //public override string Title => "Total exposure per body weight/day for modelled foods";

        public override string ChartId {
            get {
                var pictureId = "0985c652-973b-41db-9879-3e23956a50c4";
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
            var palette = CustomPalettes.GreenToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}
