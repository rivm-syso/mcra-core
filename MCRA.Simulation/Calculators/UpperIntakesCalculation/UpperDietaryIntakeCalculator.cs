﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.UpperIntakesCalculation {

    /// <summary>
    /// Returns the upper tail for acute or chronic based on dietary intakes
    /// </summary>
    public class UpperDietaryIntakeCalculator {

        private ExposureType _exposureType;

        public UpperDietaryIntakeCalculator(ExposureType exposureType) {
            _exposureType = exposureType;
        }

        /// <summary>
        /// Returns the upper percentile of the individual day intakes based on the (acute or chronic) dietary exposures.
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <returns></returns>
        public List<DietaryIndividualDayIntake> GetUpperIntakes(
                ICollection<DietaryIndividualDayIntake> intakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                double percentageForUpperTail,
                bool isPerPerson
            ) {
            if (_exposureType == ExposureType.Acute) {
                return calculateAcute(
                    intakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    percentageForUpperTail,
                    isPerPerson
                );
            } else {
                return calculateChronic(
                    intakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    percentageForUpperTail,
                    isPerPerson
                );
            }
        }

        /// <summary>
        /// Select intake days based on OIM dietary exposures.
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <returns></returns>
        private List<DietaryIndividualDayIntake> calculateChronic(
                ICollection<DietaryIndividualDayIntake> intakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                double percentageForUpperTail,
                bool isPerPerson
            ) {
            var oims = intakes
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => (
                    simulatedIndividualId: g.Key,
                    samplingWeight: g.First().IndividualSamplingWeight,
                    exposure: g.Average(idi => idi.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                ))
                .ToList();

            var exposures = oims.Select(c => c.exposure).ToList();
            var weights = oims.Select(c => c.samplingWeight).ToList();
            var intakeValue = exposures.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualIds = oims
                .Where(c => c.exposure > intakeValue)
                .Select(c => c.simulatedIndividualId)
                .ToHashSet();
            return intakes.Where(c => individualIds.Contains(c.SimulatedIndividualId)).ToList();
        }

        /// <summary>
        /// Select individual day intakes based on acute dietary exposures.
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <returns></returns>
        private List<DietaryIndividualDayIntake> calculateAcute(
                ICollection<DietaryIndividualDayIntake> intakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                double percentageForUpperTail,
                bool isPerPerson
            ) {
            var dietaryIntake = intakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var dietaryIntakeWeights = intakes.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = dietaryIntake.PercentilesWithSamplingWeights(dietaryIntakeWeights, percentageForUpperTail);
            return intakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .ToList();
        }
    }
}
