using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelAssistedChartCreator : HistogramChartCreatorBase {

        private ModelAssistedDistributionSection _section;
        private string _intakeUnit;

        public ModelAssistedChartCreator(ModelAssistedDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "1100be87-568f-439b-8c9a-43a3eefc765c";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Model assisted usual exposure distribution";

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
                $"Exposure ({intakeUnit})");
            return plotModel;
        }
    }
}

