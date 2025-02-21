using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalChronicStackedHistogramUpperChartCreator : StackedHistogramChartCreatorBase {

        private readonly InternalChronicDistributionUpperSection _section;
        private readonly string _intakeUnit;

        public InternalChronicStackedHistogramUpperChartCreator(InternalChronicDistributionUpperSection section, string intakeUnit) {
            ShowContributions = false;
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "b0cf0488-42f2-4e09-8b3e-a1627c8b7fe7";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Chronic transformed exposure distribution  ({_section.UpperPercentage:F1}%).";

        public override PlotModel Create() {
            return create(
                _section.CategorizedHistogramBins,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}