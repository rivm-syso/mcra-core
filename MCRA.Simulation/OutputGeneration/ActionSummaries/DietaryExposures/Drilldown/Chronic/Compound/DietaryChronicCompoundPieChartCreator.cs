using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicCompoundPieChartCreator : ReportPieChartCreatorBase {

        private readonly List<IndividualSubstanceDrillDownRecord> _records;
        private readonly int _ix;

        public DietaryChronicCompoundPieChartCreator(List<IndividualSubstanceDrillDownRecord> records, int ix) {
            Width = 500;
            Height = 350;
            _records = records;
            _ix = ix;
        }

        public override string Title => "Total exposure per body weight/day for substances";
        public override string ChartId {
            get {
                var pictureId = "e5b442ea-21c6-45fb-b654-bab07e655586";
                return StringExtensions.CreateFingerprint(_ix + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _records
                .GroupBy(dd => dd.SubstanceName)
                .Select(g => (
                    SubstanceName: g.Key,
                    IntakePerBodyWeight: g.Sum(s => s.EquivalentExposure))
                )
                .OrderByDescending(c => c.IntakePerBodyWeight).ToList();
            var pieSlices = records.Select(c => new PieSlice(c.SubstanceName, c.IntakePerBodyWeight)).ToList();
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
