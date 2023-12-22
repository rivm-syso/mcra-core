using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteCompoundPieChartCreator : ReportPieChartCreatorBase {

        private DietaryAcuteDrillDownRecord _record;

        public DietaryAcuteCompoundPieChartCreator(DietaryAcuteDrillDownRecord record) {
            Width = 500;
            Height = 350;
            _record = record;
        }
        public override string Title => "Total exposure per body weight/day for substances";

        public override string ChartId {
            get {
                var pictureId = "0bd4966f-a97c-4cc6-b755-54024a009188";
                return StringExtensions.CreateFingerprint(_record.Guid + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _record.IntakeSummaryPerCompoundRecords.OrderByDescending(r => r.DietaryIntakeAmountPerBodyWeight).ToList();
            var pieSlices = records.Select(c => new PieSlice(c.CompoundName, c.DietaryIntakeAmountPerBodyWeight)).ToList();
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
