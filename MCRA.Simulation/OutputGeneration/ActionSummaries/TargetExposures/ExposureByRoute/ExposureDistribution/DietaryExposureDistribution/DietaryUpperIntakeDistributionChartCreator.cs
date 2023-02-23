using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryUpperIntakeDistributionChartCreator : HistogramChartCreatorBase {

        private readonly DietaryUpperIntakeDistributionSection _section;
        private readonly string _intakeUnit;

        public DietaryUpperIntakeDistributionChartCreator(DietaryUpperIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "31eda8a5-2af6-4f82-abd0-0833f667b0dc";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Transformed upper dietary exposure distribution";

        public override PlotModel Create() {
            return createPlotModel(_section.IntakeDistributionBins, string.Empty, $"exposure ({_intakeUnit})");
        }
    }
}
