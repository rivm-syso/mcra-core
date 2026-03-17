using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalStratifiedCumulativeDistributionChartCreator : CumulativeLineChartCreatorBase {

        private readonly InternalDistributionTotalSection _section;
        private readonly string _intakeUnit;

        public InternalStratifiedCumulativeDistributionChartCreator(InternalDistributionTotalSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "caeff6c6-925d-4db1-8e04-f246cf694f82";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Stratified cumulative exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";

        public override PlotModel Create() {
            return base.createPlotModel(
               _section.StratifiedPercentiles,
               _section.UncertaintyLowerLimit,
               _section.UncertaintyUpperLimit,
               $"exposure ({_intakeUnit})"
           );
        }
    }
}
