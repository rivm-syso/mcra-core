using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public sealed class DietaryUpperIntakeCoExposureDistributionSection : DietaryUpperIntakeDistributionSection {

        /// <summary>
        /// Summarizes this section based on the main simulation run.
        /// </summary>
        /// <param name="coExposureInds"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="uncertaintyLowerLimit"></param>
        /// <param name="uncertaintyUpperLimit"></param>
        public void Summarize(
            HashSet<int> coExposureInds,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var dietaryIntakes = dietaryIndividualDayIntakes
                .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .ToList();
            var weights = dietaryIndividualDayIntakes
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var intakeValue = dietaryIntakes.PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperIntakes = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .ToList();

            if (upperIntakes.Any()) {
                CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight)
                    / dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
                var dietaryUpperIntakes = upperIntakes
                    .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                    .ToList();
                LowPercentileValue = dietaryUpperIntakes.Min();
                HighPercentileValue = dietaryUpperIntakes.Max();
            }
            NRecords = upperIntakes.Count;
            Summarize(
                coExposureInds,
                upperIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                null,
                isPerPerson,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
        }
    }
}
