using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes for processed modelled foods, substance and processing type the relative contribution to the upper tail of the exposure distribution and other statistics.
    /// </summary>
    public sealed class UpperDistributionModelledFoodSubstanceProcessingTypeSection : DistributionFoodAsMeasuredSubstanceProcessingTypeSectionBase {

        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ICollection<Food> modelledFoods,
            ICollection<ProcessingType> processingType,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double percentageForUpperTail,
            bool isPerPerson
         ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            UncertaintyLowerBound = uncertaintyLowerBound;
            UncertaintyUpperBound = uncertaintyUpperBound;
            UncertaintyCycles = 0;
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(
                dietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                percentageForUpperTail,
                isPerPerson
            );

            if (exposureType == ExposureType.Acute) {
                Records = summarizeAcute(
                    upperIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    substances,
                    modelledFoods,
                    processingType,
                    isPerPerson
                );
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
            } else {
                Records = summarizeChronic(
                    upperIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    substances, 
                    isPerPerson
                );
                NRecords = upperIntakes.Select(c => c.SimulatedIndividualId).Distinct().Count();
                if (NRecords > 0) {
                    var oims = upperIntakes
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => c.Average(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }
           
            UpperPercentage = 100 - upperIntakes.Sum(c => c.IndividualSamplingWeight) / dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
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
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, UpperPercentage, isPerPerson);

            UncertaintyCycles++;
            if (exposureType == ExposureType.Acute) {
                var records = summarizeAcute(
                    upperIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    substances, 
                    null, 
                    null, 
                    isPerPerson
                );
                updateContributions(records);
            } else {
                var records = summarizeChronic(
                    upperIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    substances, 
                    isPerPerson
                );
                updateContributions(records);
            }
        }
    }
}
