using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    /// <summary>
    /// Returns the upper tail for acute or chronic based on nondietary intakes
    /// </summary>
    public class NonDietaryUpperExposuresCalculator {

        /// <summary>
        /// Returns the upper tail exposures of the provided non-dietary exposures.
        /// </summary>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="exposureType"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        /// 
        /// <returns></returns>
        public List<NonDietaryIndividualDayIntake> GetUpperIntakes(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double upperPercentage,
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                return calculateAcute(nonDietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, upperPercentage, isPerPerson);
            } else {
                return calculateChronic(nonDietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, upperPercentage, isPerPerson);
            }
        }

        /// <summary>
        /// Select intake days based on individual mean exposures.
        /// </summary>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="upperPercentage"></param>
        /// <returns></returns>
        private List<NonDietaryIndividualDayIntake> calculateChronic(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var oims = nonDietaryIndividualDayIntakes
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => new  {
                    simulatedIndividualId = g.Key,
                    samplingWeight = g.First().IndividualSamplingWeight,
                    exposure = g.Average(idi =>  idi.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                })
                .ToList();

            var exposures = oims.Select(c => c.exposure).ToList();
            var weights = oims.Select(c => c.samplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, upperPercentage);
            var individualsId = oims.Where(c => c.exposure >= intakeValue).Select(c => c.simulatedIndividualId).ToList();
            return nonDietaryIndividualDayIntakes.Where(c =>individualsId.Contains(c.SimulatedIndividualId)).ToList();
        }

        private List<NonDietaryIndividualDayIntake> calculateAcute(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double upperPercentage,
            bool isPerPerson
        ) {
            var nondietaryIntakes = nonDietaryIndividualDayIntakes.Select(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var nondietaryIntakeWeights = nonDietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = nondietaryIntakes.PercentilesWithSamplingWeights(nondietaryIntakeWeights, upperPercentage);
            return nonDietaryIndividualDayIntakes
                .Where(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .ToList();
        }
    }
}
