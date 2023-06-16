using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk {
    public sealed class SubstanceDetailSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public string Code { get; set; }
        public string Name { get; set; }

        public void Summarize(
                SectionHeader header,
                List<IndividualEffect> individualEffects,
                Compound substance,
                double[] selectedPercentiles,
                double confidenceInterval,
                double threshold,
                HealthEffectType healthEffectType,
                RiskMetricType riskMetricType,
                bool isInverseDistribution,
                RiskMetricCalculationType riskMetricCalculationType
            ) {
            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new ThresholdExposureRatioDistributionSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Graphs", 1);
                section.Summarize(
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    isInverseDistribution,
                    selectedPercentiles,
                    individualEffects,
                    null,
                    RiskMetricCalculationType.RPFWeighted
                );
                subHeader.SaveSummarySection(section);
            }
            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new ThresholdExposureRatioPercentileSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", 1);
                section.Summarize(
                    individualEffects,
                    selectedPercentiles.Select(c => 100 - c).Reverse().ToList(),
                    null,
                    healthEffectType,
                    RiskMetricCalculationType.RPFWeighted,
                    isInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureThresholdRatioDistributionSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Graphs", 1);
                section.Summarize(
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    isInverseDistribution,
                    selectedPercentiles,
                    individualEffects,
                    null,
                    RiskMetricCalculationType.RPFWeighted
                );
                subHeader.SaveSummarySection(section);
            }
            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var section = new ExposureThresholdRatioPercentileSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", 1);
                section.Summarize(
                    individualEffects,
                    selectedPercentiles,
                    null,
                    healthEffectType,
                    RiskMetricCalculationType.RPFWeighted,
                    isInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }

            Code = substance.Code;
            Name = substance.Name;
        }

        public void SummarizeUncertainty(
            SectionHeader header,
            List<IndividualEffect> individualEffects,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {

            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<ThresholdExposureRatioDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ThresholdExposureRatioDistributionSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var subHeader = header.GetSubSectionHeader<ThresholdExposureRatioPercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ThresholdExposureRatioPercentileSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<ExposureThresholdRatioDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureThresholdRatioDistributionSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }

            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var subHeader = header.GetSubSectionHeader<ExposureThresholdRatioPercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ExposureThresholdRatioPercentileSection;
                    section.SummarizeUncertainty(
                        individualEffects,
                        isInverseDistribution,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }
    }
}
