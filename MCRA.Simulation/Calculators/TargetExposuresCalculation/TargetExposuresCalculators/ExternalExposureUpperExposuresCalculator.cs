using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {

    /// <summary>
    /// Returns the upper tail for acute or chronic based on external intakes
    /// </summary>
    public class ExternalExposureUpperExposuresCalculator {

        /// <summary>
        /// Returns the upper tail exposures of the provided external exposures.
        /// </summary>
        public List<IExternalIndividualDayExposure> GetUpperExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double upperPercentage,
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                return calculateAcute(
                    externalIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    upperPercentage,
                    isPerPerson
                );
            } else {
                return calculateChronic(
                    externalIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    upperPercentage,
                    isPerPerson
                );
            }
        }

        private List<IExternalIndividualDayExposure> calculateChronic(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var oims = externalIndividualDayExposures
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => new  {
                    simulatedIndividualId = g.Key,
                    samplingWeight = g.First().IndividualSamplingWeight,
                    exposure = g.Average(idi =>  idi.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                })
                .ToList();

            var exposures = oims.Select(c => c.exposure).ToList();
            var weights = oims.Select(c => c.samplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = oims.Where(c => c.exposure >= intakeValue).Select(c => c.simulatedIndividualId).ToList();
            return externalIndividualDayExposures.Where(c => individualsId.Contains(c.SimulatedIndividualId)).ToList();
        }

        private List<IExternalIndividualDayExposure> calculateAcute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var totalExposures = externalIndividualDayExposures
                .Select(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .ToList();
            var samplingWeights = externalIndividualDayExposures
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var intakeValue = totalExposures.PercentilesWithSamplingWeights(samplingWeights, upperPercentage);
            return externalIndividualDayExposures
                .Where(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .ToList();
        }
    }
}
