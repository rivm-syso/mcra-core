using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed non-dietary exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public sealed class NonDietaryTotalIntakeDistributionSection : NonDietaryDistributionBase, IIntakeDistributionSection {

        /// <summary>
        /// Summarizes this section based on the main simulation run.
        /// </summary>
        /// <param name="coExposureIntakes"></param>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="percentages"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="uncertaintyLowerLimit"></param>
        /// <param name="uncertaintyUpperLimit"></param>
        /// 
        public void Summarize(
            HashSet<int> coExposureIntakes,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            base.Summarize(
                coExposureIntakes,
                nonDietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                percentages,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit,
                isPerPerson);
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute). ercentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// 
        public void SummarizeUncertainty(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var weights = nonDietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            _percentiles.AddUncertaintyValues(nonDietaryIndividualDayIntakes
                .Select(i => i.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .PercentilesWithSamplingWeights(weights, _percentiles.XValues));
        }
    }
}
