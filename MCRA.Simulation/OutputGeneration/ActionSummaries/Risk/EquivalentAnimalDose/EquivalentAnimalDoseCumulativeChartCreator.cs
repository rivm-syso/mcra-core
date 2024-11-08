using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EquivalentAnimalDoseCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private EquivalentAnimalDoseSection _section;
        private string _concentrationUnit;

        public EquivalentAnimalDoseCumulativeChartCreator(EquivalentAnimalDoseSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _concentrationUnit = intakeUnit;
        }

        public override string Title => $"Cumulative equivalent animal dose distribution ({100:F1}% positives)";

        public override string ChartId {
            get {
                var pictureId = "07b331ea-bf71-461f-acaf-9243239f611a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.PercentilesGrid,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Equivalent animal dose ({_concentrationUnit})"
            );
        }
    }
}
