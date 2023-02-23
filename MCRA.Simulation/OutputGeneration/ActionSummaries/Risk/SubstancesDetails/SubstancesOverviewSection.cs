using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk {
    public sealed class SubstancesOverviewSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public void Summarize(
                SectionHeader header,
                Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
                ICollection<Compound> activeSubstances,
                double confidenceInterval,
                double thresholdMarginOfExposure,
                HealthEffectType healthEffectType,
                RiskMetricType riskMetricType,
                bool isInverseDistribution,
                double[] selectedPercentiles
            ) {
            var count = 0;
            foreach (var substance in activeSubstances) {
                var individualEffects = substanceIndividualEffects[substance];
                var section = new SubstanceDetailSection();
                var subHeader = header.AddSubSectionHeaderFor(section, getSubSectionTitle(substance), count++);
                section.Summarize(
                    subHeader,
                    individualEffects,
                    substance,
                    selectedPercentiles,
                    confidenceInterval,
                    thresholdMarginOfExposure,
                    healthEffectType,
                    riskMetricType,
                    isInverseDistribution
                );
                subHeader.SaveSummarySection(section);
            }
        }

        public void SummarizeUncertain(
            SectionHeader header,
            ICollection<Compound> activeSubstances,
            Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            foreach (var substance in activeSubstances) {
                var individualEffects = substanceIndividualEffects[substance];
                var title = getSubSectionTitle(substance);
                var subHeader = header.GetSubSectionHeaderFromTitleString<SubstanceDetailSection>(title);
                var section = subHeader?.GetSummarySection() as SubstanceDetailSection ?? null;
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
