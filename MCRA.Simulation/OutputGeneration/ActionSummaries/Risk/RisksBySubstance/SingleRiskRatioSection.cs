using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleRiskRatioSection : RiskCharacterisationRatioSectionBase<RiskSubstanceContributorKey, SubstanceRiskDistributionRecord> {

        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// Cumulative risk record / single substance total risk record
        /// </summary>
        public SubstanceRiskDistributionRecord SingleRiskRecord { get; set; }

        /// <summary>
        /// Stratified cumulative risk record / single substance total risk record
        /// </summary>
        public List<SubstanceRiskDistributionRecord> StratifiedSingleRiskRecords { get; set; } = [];

        public void Summarize(
            List<IndividualEffect> individualEffects,
            TargetUnit targetUnit,
            Compound substance,
            Effect effect,
            double confidenceInterval,
            double threshold,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            IHazardCharacterisationModel referenceDose,
            double leftMargin,
            double rightMargin,
            bool isInverseDistribution,
            bool isCumulative,
            PopulationStratifier stratifier,
            bool skipPrivacySensitiveOutputs
        ) {
            var pLower = (100 - confidenceInterval) / 2;
            var pUpper = 100 - pLower;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(individualEffects.Count);
                if (pUpper > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                    pUpper = maxUpperPercentile;
                    pLower = 100 - pUpper;
                    confidenceInterval = 100 - 2 * pLower;
                }
            }
            TargetUnit = targetUnit;
            StratificationVariables = stratifier?.GetStratificationVariables();
            EffectName = effect?.Name;
            RiskMetric = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            ConfidenceInterval = confidenceInterval;
            ReferenceDose = referenceDose?.Value ?? double.NaN;
            Threshold = threshold;
            LeftMargin = leftMargin;
            RightMargin = rightMargin;
            RiskBarPercentages = [pLower, 50, pUpper];

            var effects = getIndividualEffects(individualEffects, stratifier);
            var records = createSingleRiskRecords(
                targetUnit?.Target,
                substance,
                effects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative,
                stratify: false
            );
            SingleRiskRecord = records.First();
            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                var stratifiedRecords = createSingleRiskRecords(
                    targetUnit?.Target,
                    substance,
                    effects,
                    riskMetricCalculationType,
                    isInverseDistribution,
                    isCumulative,
                    stratify: true
                );
                StratifiedSingleRiskRecords.AddRange(stratifiedRecords);
            }
        }

        /// <summary>
        /// Summarizes uncertainty for risks safety charts single substance.
        /// </summary>
        public void SummarizeUncertain(
            Compound substance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative,
            PopulationStratifier stratifier
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;

            var effects = getIndividualEffects(individualEffects, stratifier);
            var uncertaintyRecord = createSingleRiskRecords(
                TargetUnit.Target,
                substance,
                effects,
                riskMetricCalculationType,
                isInverseDistribution,
                isCumulative,
                stratify: false
            ).First();
            SingleRiskRecord.UncertaintyLowerLimit = uncertaintyLowerBound;
            SingleRiskRecord.UncertaintyUpperLimit = uncertaintyUpperBound;
            SingleRiskRecord.RiskPercentiles.AddUncertaintyValues([uncertaintyRecord.PLowerRiskNom, uncertaintyRecord.RiskP50Nom, uncertaintyRecord.PUpperRiskNom]);
            SingleRiskRecord.ProbabilityOfCriticalEffects.AddUncertaintyValues([uncertaintyRecord.ProbabilityOfCriticalEffect]);

            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                var stratifiedRecords = createSingleRiskRecords(
                    TargetUnit.Target,
                    substance,
                    effects,
                    riskMetricCalculationType,
                    isInverseDistribution,
                    isCumulative,
                    stratify: true
                );
                for (int i = 0; i < stratifiedRecords.Count; i++) {
                    var stratifiedRecord = stratifiedRecords[i];
                    var existingRecord = StratifiedSingleRiskRecords.FirstOrDefault(r =>
                        r.Stratification == stratifiedRecord.Stratification &&
                        r.StratificationLevelCode == stratifiedRecord.StratificationLevelCode
                    );
                    if (existingRecord != null) {
                        existingRecord.UncertaintyLowerLimit = uncertaintyLowerBound;
                        existingRecord.UncertaintyUpperLimit = uncertaintyUpperBound;
                        existingRecord.RiskPercentiles.AddUncertaintyValues([stratifiedRecord.PLowerRiskNom, stratifiedRecord.RiskP50Nom, stratifiedRecord.PUpperRiskNom]);
                        existingRecord.ProbabilityOfCriticalEffects.AddUncertaintyValues([stratifiedRecord.ProbabilityOfCriticalEffect]);
                    }
                }
            }
        }

        private List<SubstanceRiskDistributionRecord> createSingleRiskRecords(
            ExposureTarget target,
            Compound substance,
            ICollection<IndividualEffectsByStratificationLevel> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative,
            bool stratify
        ) {
            var results = new List<SubstanceRiskDistributionRecord>();
            var descriptor = new RiskSubstanceContributorKey() {
                Substance = isCumulative ? getRiskReference(riskMetricCalculationType) : substance
            };
            var groups = individualEffects
                .GroupBy(c => stratify ? c.StratificationLevel : null);
            foreach (var group in groups) {
                var record = createSubstanceRiskRecord(
                    target,
                    [.. group],
                    isCumulative,
                    isInverseDistribution,
                    descriptor,
                    group.Key
                );
                results.Add(record);
            }
            return results;
        }
    }
}
