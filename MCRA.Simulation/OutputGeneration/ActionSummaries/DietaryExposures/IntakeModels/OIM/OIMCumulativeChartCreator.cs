using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OIMCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private OIMDistributionSection _section;
        private string _intakeUnit;

        public OIMCumulativeChartCreator(OIMDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "d7a727ab-e687-40d7-8dbc-f6ab1b980da7";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Cumulative exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";

        public override PlotModel Create() {
            return createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}
