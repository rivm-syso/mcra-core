using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalChronicDistributionUpperChartCreator : ExposureHistogramChartCreatorBase {

        private readonly InternalChronicDistributionUpperSection _section;
        private readonly string _intakeUnit;

        public InternalChronicDistributionUpperChartCreator(InternalChronicDistributionUpperSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string Title => $"Transformed upper exposure distribution ({_section.UpperPercentage:F1}%).";

        public override string ChartId {
            get {
                var pictureId = "668e7faf-9929-4a02-834b-127a908076cc";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(
                _section.IntakeDistributionBins,
                _section.IntakeDistributionBinsCoExposure,
                string.Empty,
                _intakeUnit
            );
        }
    }
}
