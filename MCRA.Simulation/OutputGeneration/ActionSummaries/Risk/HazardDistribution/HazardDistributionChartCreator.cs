using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardDistributionChartCreator : HistogramChartCreatorBase {

        private HazardDistributionSection _section;
        private string _concentrationUnit;

        public HazardDistributionChartCreator(HazardDistributionSection section, string exposureUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _concentrationUnit = exposureUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "66da9744-8741-4a54-899c-6d115337ae66";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Individual critical effect dose distribution.";
        public override PlotModel Create() {
            return create(_section.CEDDistributionBins, _concentrationUnit);
        }

        private PlotModel create(List<HistogramBin> binsTransformed, string exposureUnit) {
            var xtitle = $"Individual critical effect dose ({exposureUnit})";
            return base.createPlotModel(binsTransformed, string.Empty, xtitle);
        }
    }
}
