using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AcuteUpperStackedHistogramChartCreator : StackedHistogramChartCreatorBase {

        private AggregateUpperIntakeDistributionSection _section;
        private string _intakeUnit;

        public AcuteUpperStackedHistogramChartCreator(AggregateUpperIntakeDistributionSection section, string intakeUnit) {
            ShowContributions = false;
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "e8f7e1c9-ffd9-4f0b-b016-1cb1975ae1c7";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Acute transformed exposure distribution  ({100 - _section.PercentageZeroIntake:F1} % positives)";

        public override PlotModel Create() {
            return create(_section.AcuteCategorizedHistogramBins, _section.PercentageZeroIntake / 100D, _intakeUnit);
        }

        protected override PlotModel create<T>(List<CategorizedHistogramBin<T>> binsTransformed, double fractionZeros, string intakeUnit) {
            var plotModel = base.create(binsTransformed, fractionZeros, intakeUnit);
            return plotModel;
        }
    }
}