using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AggregateStackedHistogramChartCreator : StackedHistogramChartCreatorBase {

        private OIMDistributionSection _section;
        private string _intakeUnit;

        public AggregateStackedHistogramChartCreator(OIMDistributionSection section, string intakeUnit) {
            ShowContributions = false;
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "4244e496-1685-4711-91f0-abed63c19c41";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Stacked transformed exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";
        public override PlotModel Create() {
            return create(_section.CategorizedHistogramBins, _section.PercentageZeroIntake / 100D, _intakeUnit);
        }

        protected override PlotModel create<T>(List<CategorizedHistogramBin<T>> binsTransformed, double fractionZeros, string intakeUnit) {
            var plotModel = base.create(binsTransformed, fractionZeros, intakeUnit);
            return plotModel;
        }
    }
}