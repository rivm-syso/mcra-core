﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the upper tail of the transformed non-dietary exposure distribution in bins,
    /// is used for plotting of the transformed exposure distribution
    /// </summary>
    public sealed class NonDietaryUpperIntakeDistributionSection : NonDietaryDistributionBase {

        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }

        public void Summarize(
            HashSet<int> coExposureIds,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var nonDietaryIntakes = nonDietaryIndividualDayIntakes.Select(c => (
                TotalNonDietaryIntake: c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson),
                SamplingWeight: c.IndividualSamplingWeight
            ));
            var nonDietaryIntakeWeights = nonDietaryIntakes.Select(c => c.SamplingWeight).ToList();
            var intakeValue = Stats.PercentilesWithSamplingWeights(nonDietaryIntakes.Select(c => c.TotalNonDietaryIntake), nonDietaryIntakeWeights, percentageForUpperTail);
            var upperIntakes = nonDietaryIndividualDayIntakes
                .Where(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > intakeValue)
                .ToList();
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / nonDietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
            Summarize(
                coExposureIds,
                upperIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                null,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit,
                isPerPerson);
        }
    }
}
