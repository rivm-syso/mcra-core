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
                bool isInverseDistribution
            ) {
            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new MarginOfExposureDistributionSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Graphs", 1);
                section.Summarize(
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    isInverseDistribution,
                    selectedPercentiles,
                    individualEffects,
                    null
                );
                subHeader.SaveSummarySection(section);
            }
            if (individualEffects != null && riskMetricType == RiskMetricType.MarginOfExposure) {
                var section = new MarginOfExposurePercentileSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", 1);
                section.Summarize(
                    individualEffects,
                    selectedPercentiles.Select(c => 100 - c).Reverse().ToList(),
                    null,
                    healthEffectType,
                    isInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var section = new HazardIndexDistributionSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Graphs", 1);
                section.Summarize(
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    isInverseDistribution,
                    selectedPercentiles,
                    individualEffects,
                    null
                );
                subHeader.SaveSummarySection(section);
            }
            if (individualEffects != null && riskMetricType == RiskMetricType.HazardIndex) {
                var section = new HazardIndexPercentileSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", 1);
                section.Summarize(
                    individualEffects,
                    selectedPercentiles,
                    null,
                    healthEffectType,
                    isInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }

            Code = $"{substance.Code}";
            Name = $"{substance.Name}";
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
                var subHeader = header.GetSubSectionHeader<MarginOfExposureDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MarginOfExposureDistributionSection;
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
                var subHeader = header.GetSubSectionHeader<MarginOfExposurePercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as MarginOfExposurePercentileSection;
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
                var subHeader = header.GetSubSectionHeader<HazardIndexDistributionSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardIndexDistributionSection;
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
                var subHeader = header.GetSubSectionHeader<HazardIndexPercentileSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as HazardIndexPercentileSection;
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
