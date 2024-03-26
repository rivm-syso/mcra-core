using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk {
    public sealed class SubstancesOverviewSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public void Summarize(
            SectionHeader header,
            ICollection<ExposureTarget> targets,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            ICollection<Compound> activeSubstances,
            double confidenceInterval,
            double threshold,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            double[] selectedPercentiles,
            bool skipPrivacySensitiveOutputs
        ) {
            var count = 0;
            foreach (var target in targets) {
                foreach (var substance in activeSubstances) {
                    var individualEffectsDict = individualEffectsBySubstanceCollections
                        .SingleOrDefault(c => c.Target == target).IndividualEffects;
                    if (individualEffectsDict.TryGetValue(substance, out var individualEffects)) {
                        if (!individualEffectsDict[substance].All(c => !c.IsPositive)) {
                            var section = new SubstanceDetailSection();
                            var title = targets.Count > 1
                                ? $"{getSubSectionTitle(substance)} ({target.GetDisplayName()})"
                                : getSubSectionTitle(substance);
                            var subHeader = header.AddSubSectionHeaderFor(section, title, count++);
                            section.Summarize(
                                subHeader,
                                individualEffects,
                                substance,
                                selectedPercentiles,
                                confidenceInterval,
                                threshold,
                                riskMetricType,
                                isInverseDistribution,
                                skipPrivacySensitiveOutputs
                            );
                            subHeader.SaveSummarySection(section);
                        }
                    }
                }
            }
        }

        public void SummarizeUncertain(
            SectionHeader header,
            ICollection<ExposureTarget> targets,
            ICollection<Compound> activeSubstances,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            foreach (var target in targets) {
                foreach (var substance in activeSubstances) {
                    var individualEffectsDict = individualEffectsBySubstanceCollections
                        .SingleOrDefault(c => c.Target == target).IndividualEffects;
                    if (individualEffectsDict.TryGetValue(substance, out var individualEffects)) {
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
            }
        }

        public string getSubSectionTitle(Compound substance) {
            return $"{substance.Name}, {substance.Code}";
        }
    }
}
