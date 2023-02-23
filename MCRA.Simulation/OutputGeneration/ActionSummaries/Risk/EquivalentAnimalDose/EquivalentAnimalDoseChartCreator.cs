using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EquivalentAnimalDoseChartCreator : HistogramChartCreatorBase {

        private EquivalentAnimalDoseSection _section;
        private string _concentrationUnit;

        public EquivalentAnimalDoseChartCreator(EquivalentAnimalDoseSection section, string concentrationUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _concentrationUnit = concentrationUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "9db2524c-40e8-4f81-a8d1-f81243c6a312";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.EADDistributionBins, _section.PercentageZeroIntake, _concentrationUnit);
        }

        private PlotModel create(List<HistogramBin> binsTransformed, double percentageZeros, string concentrationUnit) {
            var title = $"Equivalent animal dose distribution ({100 - percentageZeros:F1}% positives) ";
            var xtitle = $"Equivalent animal dose ({concentrationUnit})";
            return base.createPlotModel(binsTransformed, title, xtitle);
        }
    }
}
