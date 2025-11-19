using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {

    public class CumulativeTargetExposuresCalculator {

        public static Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> ComputeCumulativeExposures(
            Dictionary<ExposureTarget, (List<ITargetIndividualExposure> targetIndividualExposures, TargetUnit targeUnit)> substanceTargetExposures,
            Compound referenceSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            var result = new Dictionary<ExposureTarget, (List<ITargetIndividualExposure>, TargetUnit)>();
            foreach (var item in substanceTargetExposures) {
                var targetIndividualExposures = item.Value.targetIndividualExposures;
                var cumulativeExposure = targetIndividualExposures
                    .Select(c => {
                        var targetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure> {
                            [referenceSubstance] = new SubstanceTargetExposure() {
                                Substance = referenceSubstance,
                                Exposure = c.GetCumulativeExposure(relativePotencyFactors, membershipProbabilities)
                            }
                        };
                        return new TargetIndividualExposure() {
                            SimulatedIndividual = c.SimulatedIndividual,
                            TargetExposuresBySubstance = targetExposuresBySubstance
                        } as ITargetIndividualExposure;
                    })
                    .ToList();
                result[item.Key] = (cumulativeExposure, item.Value.targeUnit);

            }
            return result;
        }
    }
}
