using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class CombinedRisksViolinChartCreator(
       CombinedRiskPercentilesSection section,
       double percentile,
       bool horizontal,
       bool boxplotItem,
       bool equalSize
    ) : CombinedViolinChartCreatorBase(section, percentile, horizontal, boxplotItem, equalSize) {
        private RiskMetricType RiskType { get; } = section.RiskMetric;

        public override string ChartId {
            get {
                var pictureId = "d9808ef6-d58f-44a0-8f52-f72302438447";
                return StringExtensions.CreateFingerprint(_section.SectionId + _percentile + pictureId);
            }
        }

        public override string Title {
            get {
                return $"Violin plots of the uncertainty distribution of the {RiskType.GetDisplayName().ToLower()} " +
                    $"at the p{_percentile} percentile of the population risk distributions. The vertical lines " +
                    $"represent the median and the lower p{_lowerBound} and upper p{_upperBound} bound of the " +
                    $"uncertainty distribution. The nominal run is indicated by the black dot.";
            }
        }

        protected override string HorizontalAxisTitle => $"{RiskType.GetDisplayName()}";
    }
}

