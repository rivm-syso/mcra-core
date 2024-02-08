using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes for modelled foods, substance and processing type the relative contribution to the upper tail of the exposure distribution and other statistics.
    /// </summary>
    public sealed class TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSection : DistributionFoodAsMeasuredSubstanceProcessingTypeSectionBase {

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ICollection<Food> modelledFoods,
            ICollection<ProcessingType> processingTypes,
            ExposureType exposureType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
         ) {
            UncertaintyLowerBound = uncertaintyLowerBound;
            UncertaintyUpperBound = uncertaintyUpperBound;
            UncertaintyCycles = 0;
            if (exposureType == ExposureType.Acute) {
                Records = summarizeAcute(
                    dietaryIndividualDayIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    substances, 
                    modelledFoods,
                    processingTypes,
                    isPerPerson
                );
            } else {
                Records = summarizeChronic(
                    dietaryIndividualDayIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    substances, 
                    isPerPerson
                );
            }
            setUncertaintyBounds();
        }

        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            UncertaintyCycles++;
            if (exposureType == ExposureType.Acute) {
                var records = summarizeAcute(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, substances, null, null, isPerPerson);
                updateContributions(records);
            } else {
                var records = summarizeChronic(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, substances, isPerPerson);
                updateContributions(records);
            }
        }
    }
}
