using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk {
    public sealed class SubstancesOverviewSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public void Summarize(
            SectionHeader header,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            ICollection<Compound> activeSubstances,
            double confidenceInterval,
            double threshold,
            HealthEffectType healthEffectType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            RiskMetricCalculationType riskMetricCalculationType,
            double[] selectedPercentiles
        ) {
            var count = 0;
            foreach (var substance in activeSubstances) {
                var individualEffects = individualEffectsBySubstance[substance];
                var section = new SubstanceDetailSection();
                var subHeader = header.AddSubSectionHeaderFor(section, getSubSectionTitle(substance), count++);
                section.Summarize(
                    subHeader,
                    individualEffects,
                    substance,
                    selectedPercentiles,
                    confidenceInterval,
                    threshold,
                    healthEffectType,
                    riskMetricType,
                    isInverseDistribution,
                    riskMetricCalculationType
                );
                subHeader.SaveSummarySection(section);
            }
        }

        public void SummarizeUncertain(
            SectionHeader header,
            ICollection<Compound> activeSubstances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            foreach (var substance in activeSubstances) {
                var individualEffects = individualEffectsBySubstance[substance];
                var title = getSubSectionTitle(substance);
                var subHeader = header.GetSubSectionHeaderFromTitleString<SubstanceDetailSection>(title);
                var section = subHeader?.GetSummarySection() as SubstanceDetailSection;
                if (section != null) {
                    section.SummarizeUncertainty(
                        subHeader,
                        individualEffects,
                        riskMetricType,
                        isInverseDistribution,
                        uncertaintyLowerLimit,
                        uncertaintyUpperLimit
                   );
                }
            }
        }

        public string getSubSectionTitle(Compound substance) {
            return $"{substance.Name}, {substance.Code}";
        }
    }
}
