using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardIndexCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private HazardIndexDistributionSection _section;

        public HazardIndexCumulativeChartCreator(HazardIndexDistributionSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "6e0fa885-07c0-4de3-99a7-a82de7f444d3";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Hazard index cumulative total distribution ({100 - _section.PercentageZeros:F1}% positives)";
        
        public override PlotModel Create() {
            return base.createPlotModel(
                 _section.PercentilesGrid,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Hazard index"
            );
        }
    }
}
