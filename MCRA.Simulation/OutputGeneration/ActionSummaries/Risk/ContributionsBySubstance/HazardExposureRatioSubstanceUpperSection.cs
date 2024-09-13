using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardExposureRatioSubstanceUpperSection : RiskContributionsBySubstanceSection {

        protected override RiskBySubstanceRecord createSubstanceSummaryRecord(
            List<IndividualEffect> individualEffects,
            Compound substance,
            double totalExposure
        ) {
            var (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights) = CalculatesHazardExposurePercentiles(
                individualEffects
            );
            var record = new RiskBySubstanceRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contributions = new List<double>(),
                MeanAll = weights.Any() ? total / sumSamplingWeights : SimulationConstants.MOE_eps,
                Contribution = total / totalExposure,
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
            };
            return record;
        }
    }
}
