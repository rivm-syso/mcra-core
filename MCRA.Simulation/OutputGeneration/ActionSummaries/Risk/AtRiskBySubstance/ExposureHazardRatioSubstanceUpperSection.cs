using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureHazardRatioSubstanceUpperSection : RiskRatioBySubstanceSection {

        protected override RiskBySubstanceRecord createSubstanceSummaryRecord(
            List<IndividualEffect> individualEffects,
            Compound substance,
            double riskTotal
        ) {
            var (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights) = CalculateExposureHazardPercentiles(
                individualEffects
            );
            var record = new RiskBySubstanceRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contributions = new List<double>(),
                MeanAll = weights.Any() ? total / sumSamplingWeights : 0,
                Contribution = total / riskTotal,
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
            };
            return record;
        }
    }
}
