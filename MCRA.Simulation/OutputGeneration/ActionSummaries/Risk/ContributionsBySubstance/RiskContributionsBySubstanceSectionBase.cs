using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class RiskContributionsBySubstanceSectionBase : AtRiskSectionBase {

        public override bool SaveTemporaryData => true;

        protected double _lowerPercentage;
        protected double _upperPercentage;

        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public List<RiskBySubstanceRecord> Records { get; set; }

        protected RiskBySubstanceRecord createSubstanceSummaryRecord(
            List<IndividualEffect> individualEffects,
            Compound substance,
            double totalExposure,
            RiskMetricType riskMetricType
        ) {
            var (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights) = 
                riskMetricType == RiskMetricType.HazardExposureRatio
                    ? CalculateHazardExposurePercentiles(individualEffects)
                    : CalculateExposureHazardPercentiles(individualEffects);
            var meanAll = riskMetricType == RiskMetricType.HazardExposureRatio
                ? weights.Any() ? total / sumSamplingWeights : SimulationConstants.MOE_eps
                : weights.Any() ? total / sumSamplingWeights : 0;
            var record = new RiskBySubstanceRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contributions = [],
                MeanAll = meanAll,
                Contribution = total / totalExposure,
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
            };
            return record;
        }

        protected void updateContributions(List<RiskBySubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records
                    .FirstOrDefault(c => c.SubstanceCode == record.SubstanceCode)
                    ?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }

        protected void setUncertaintyBounds(double lowerBound, double upperBound) {
            foreach (var item in Records) {
                item.UncertaintyLowerBound = lowerBound;
                item.UncertaintyUpperBound = upperBound;
            }
        }
    }
}
