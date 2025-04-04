using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryTotalIntakeCumulativeDistributionChartCreator : CumulativeLineChartCreatorBase {

        private DietaryTotalIntakeDistributionSection _section;
        private string _intakeUnit;

        public DietaryTotalIntakeCumulativeDistributionChartCreator(DietaryTotalIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "27965162-f514-420c-8896-bbbf1bfc080a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Cumulative dietary exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerlimit,
                _section.UncertaintyUpperlimit,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}
