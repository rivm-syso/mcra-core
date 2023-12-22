using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PredictedHealthEffectChartCreator : ReportHistogramChartCreatorBase {

        private PredictedHealthEffectSection _section;
        private string _concentrationUnit;

        public PredictedHealthEffectChartCreator(PredictedHealthEffectSection section, string concentrationUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _concentrationUnit = concentrationUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "879b2440-98e5-4f53-a995-44506a7960b3";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.PHEDistributionBins, _section.PercentageZeroIntake,  _concentrationUnit);
        }

        private PlotModel create(
                List<HistogramBin> binsTransformed,
                double percentageZeros,
                string concentrationUnit
            ) {
            var title = $"Health effect distribution ({100 - percentageZeros:F1}% positives)";
            var xtitle = $"Health effect ({concentrationUnit})";
            return base.createPlotModel(binsTransformed, title, xtitle);
        }
    }
}
