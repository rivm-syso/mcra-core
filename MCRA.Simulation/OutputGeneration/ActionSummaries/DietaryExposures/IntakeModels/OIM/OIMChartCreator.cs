using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OIMChartCreator : HistogramChartCreatorBase {

        private ModelBasedDistributionSection _section;
        private string _intakeUnit;

        public OIMChartCreator(ModelBasedDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "c5779b98-c157-498a-be9a-0b823b864c28";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

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
            var title = "OIM usual exposure distribution";
            var xtitle = $"Exposure ({intakeUnit})";
            return createPlotModel(
                binsTransformed,
                title,
                xtitle
            );
        }
    }
}

