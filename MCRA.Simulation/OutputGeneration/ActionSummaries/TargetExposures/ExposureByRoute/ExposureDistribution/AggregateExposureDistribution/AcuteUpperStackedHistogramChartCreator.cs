using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AcuteUpperStackedHistogramChartCreator : StackedHistogramChartCreatorBase {

        private readonly AggregateUpperIntakeDistributionSection _section;
        private readonly string _intakeUnit;

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
        public override string Title => $"Acute transformed exposure distribution  ({_section.UpperPercentage:F1}%).";

        public override PlotModel Create() {
            return create(
                _section.CategorizedHistogramBins,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}