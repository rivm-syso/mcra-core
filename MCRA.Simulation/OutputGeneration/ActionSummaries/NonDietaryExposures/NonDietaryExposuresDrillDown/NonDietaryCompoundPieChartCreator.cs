using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryCompoundPieChartCreator : ReportPieChartCreatorBase {

        private NonDietaryDrillDownRecord _record;

        public NonDietaryCompoundPieChartCreator(NonDietaryDrillDownRecord record) {
            Width = 500;
            Height = 350;
            _record = record;
        }

        public override string Title => "Total exposure per body weight/day for substances";


        public override string ChartId {
            get {
                var pictureId = "77029e32-a543-4c4e-924b-57671c9ffc95";
                return StringExtensions.CreateFingerprint(_record.Guid + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _record.NonDietaryIntakeSummaryPerCompoundRecords.OrderByDescending(r => r.NonDietaryIntakeAmountPerBodyWeight).ToList();
            var pieSlices = records.Select(c => new PieSlice(c.SubstanceName, c.NonDietaryIntakeAmountPerBodyWeight)).ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.BeachToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}