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
            UpperPercentage = percentageForUpperTail;
            var dietaryIntakes = dietaryIndividualDayIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = dietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = Stats.PercentilesWithSamplingWeights(dietaryIntakes, weights, UpperPercentage);

            var upperIntakes = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .ToList();

            if (upperIntakes.Any()) {
                UpperPercentage = 100 - upperIntakes.Sum(c => c.IndividualSamplingWeight) / dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
                LowPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                HighPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
            }
            NRecords = upperIntakes.Count;
            Summarize(coExposureInds, upperIntakes, relativePotencyFactors, membershipProbabilities, null, isPerPerson, uncertaintyLowerLimit, uncertaintyUpperLimit);
        }
    }
}
