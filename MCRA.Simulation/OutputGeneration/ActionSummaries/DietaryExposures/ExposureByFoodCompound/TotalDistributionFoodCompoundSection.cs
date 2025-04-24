using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes for modelled foods the relative contribution to the upper tail of the exposure distribution and other statistics.
    /// </summary>
    public sealed class TotalDistributionFoodCompoundSection : DistributionFoodCompoundSectionBase {

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Food> modelledFoods,
            ICollection<Compound> substances,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            Percentages = [lowerPercentage, 50, upperPercentage];
            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    modelledFoods,
                    substances,
                    isPerPerson
                );
            } else {
                Records = SummarizeChronic(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
            }
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                var distributionFoodCompoundRecords = SummarizeUncertaintyAcute(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    null,
                    substances,
                    isPerPerson
                );
                updateContributions(distributionFoodCompoundRecords);
            } else {
                var distributionFoodCompoundRecords = SummarizeUncertaintyChronic(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                updateContributions(distributionFoodCompoundRecords);
            }
        }

        private void setUncertaintyBounds(double uncertaintyLowerBound, double uncertaintyUpperBound) {
            foreach (var item in Records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }
    }
}
