using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryTotalIntakeCumulativeDistributionChartCreator : CumulativeLineChartCreatorBase {

        private NonDietaryTotalIntakeDistributionSection _section;
        private string _intakeUnit;

        public NonDietaryTotalIntakeCumulativeDistributionChartCreator(NonDietaryTotalIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }
        public override string Title => $"Non-dietary cumulative exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";
        public override string ChartId {
            get {
                var pictureId = "7c79078b-2eac-4a2b-8f7a-32f2f7f64e2c";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}
