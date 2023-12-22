using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicCompoundPieChartCreator : ReportPieChartCreatorBase {

        private DietaryChronicDrillDownRecord _record;

        public DietaryChronicCompoundPieChartCreator(DietaryChronicDrillDownRecord record) {
            Width = 500;
            Height = 350;
            _record = record;
        }

        public override string Title => "Total exposure per body weight/day for substances";
        public override string ChartId {
            get {
                var pictureId = "e5b442ea-21c6-45fb-b654-bab07e655586";
                return StringExtensions.CreateFingerprint(_record.Guid + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _record.DayDrillDownRecords
                .SelectMany(dd => dd.DietaryIntakeSummaryPerCompoundRecords)
                .GroupBy(dd => dd.CompoundName)
                .Select(g => (CompoundName: g.Key, IntakePerBodyWeight: g.Sum(s => s.DietaryIntakeAmountPerBodyWeight)))
                .OrderByDescending(c => c.IntakePerBodyWeight).ToList();
            var pieSlices = records.Select(c => new PieSlice(c.CompoundName, c.IntakePerBodyWeight)).ToList();
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
