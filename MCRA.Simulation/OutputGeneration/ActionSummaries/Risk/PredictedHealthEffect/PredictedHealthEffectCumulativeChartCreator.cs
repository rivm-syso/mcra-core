using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PredictedHealthEffectCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private PredictedHealthEffectSection _section;
        private string _concentrationUnit;

        public PredictedHealthEffectCumulativeChartCreator(PredictedHealthEffectSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _concentrationUnit = intakeUnit;
        }
        public override string Title => $"Cumulative health effect distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";

        public override string ChartId {
            get {
                var pictureId = "3c352c78-2d79-4970-b57d-9dbafebada7d";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.PercentilesGrid,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Health effect ({_concentrationUnit})"
            );
        }
    }
}
