using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryTotalIntakeDistributionChartCreator : ReportHistogramChartCreatorBase {

        private DietaryTotalIntakeDistributionSection _section;
        private string _intakeUnit;

        public DietaryTotalIntakeDistributionChartCreator(DietaryTotalIntakeDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "5bd3856f-73f3-4ec6-900f-9d6bdd0b2d8f";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Transformed dietary exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";

        public override PlotModel Create() {
            return create(
                _section.IntakeDistributionBins,
                _intakeUnit
            );
        }

        private PlotModel create(
            List<HistogramBin> binsTransformed,
            string intakeUnit
        ) {
            var plotModel = createPlotModel(
                binsTransformed,
                string.Empty,
                $"exposure ({intakeUnit})");
            return plotModel;
        }
    }
}
