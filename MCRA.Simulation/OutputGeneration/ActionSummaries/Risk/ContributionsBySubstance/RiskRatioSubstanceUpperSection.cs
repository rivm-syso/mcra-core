using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskRatioSubstanceUpperSection : RiskContributionsBySubstanceSection {

        protected override RiskBySubstanceRecord createSubstanceSummaryRecord(
            List<IndividualEffect> individualEffects,
            Compound substance,
            double totalExposure,
            RiskMetricType riskMetricType
        ) {
            var (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights) = riskMetricType == RiskMetricType.HazardExposureRatio
                ? CalculateHazardExposurePercentiles(individualEffects)
                : CalculateExposureHazardPercentiles(individualEffects);
            var meanAll = riskMetricType == RiskMetricType.HazardExposureRatio
                ? weights.Any() ? total / sumSamplingWeights : SimulationConstants.MOE_eps
                : weights.Any() ? total / sumSamplingWeights : 0;
            var record = new RiskBySubstanceRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contributions = new List<double>(),
                MeanAll = meanAll,
                Contribution = total / totalExposure,
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
            };
            return record;
        }
    }
}
