using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryTotalIntakeDistributionChartCreator : ReportHistogramChartCreatorBase {

        private readonly NonDietaryTotalIntakeDistributionSection _section;
        private readonly string _intakeUnit;

        public NonDietaryTotalIntakeDistributionChartCreator(NonDietaryTotalIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "95afe898-6016-4fd1-8c42-c9a2987bac7d";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Transformed non-dietary exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives).";

        public override PlotModel Create() {
            var xtitle = $"Exposure ({_intakeUnit})";
            return createPlotModel(_section.IntakeDistributionBins, string.Empty, xtitle);
        }
    }
}
