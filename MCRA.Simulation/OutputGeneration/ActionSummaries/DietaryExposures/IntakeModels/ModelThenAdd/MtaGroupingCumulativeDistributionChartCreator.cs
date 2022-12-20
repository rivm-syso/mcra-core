using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MtaGroupingCumulativeDistributionChartCreator : CumulativeLineChartCreatorBase {

        private readonly UsualIntakeDistributionPerCategoryModelSection _section;
        private readonly string _intakeUnit;

        public MtaGroupingCumulativeDistributionChartCreator(
            UsualIntakeDistributionPerCategoryModelSection section,
            string intakeUnit
        ) {
            Height = 400;
            Width = 400;
            _section = section;
            _intakeUnit = intakeUnit;
        }
        public override string Title => $"Model OIM exposure distribution {100 - _section.PercentageZeroIntake:F1}% positives";
        public override string ChartId {
            get {
                var pictureId = "9c090910-11fc-4a3b-9fc2-a296a783cd03";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                _intakeUnit
            );
        }
    }
}
