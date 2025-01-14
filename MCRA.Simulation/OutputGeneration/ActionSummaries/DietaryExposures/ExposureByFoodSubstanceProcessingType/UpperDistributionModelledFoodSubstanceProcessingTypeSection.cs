using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

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
            ExposureType exposureType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double percentageForUpperTail,
            bool isPerPerson
         ) {
            UncertaintyLowerBound = uncertaintyLowerBound;
            UncertaintyUpperBound = uncertaintyUpperBound;
            UncertaintyCycles = 0;
            UpperPercentage = 100 - percentageForUpperTail;
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
                    isPerPerson
                );
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    var dietaryUpperIntakes = upperIntakes
                        .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();
                    LowPercentileValue = dietaryUpperIntakes.Min();
                    HighPercentileValue = dietaryUpperIntakes.Max();
                }
            } else {
                Records = summarizeChronic(
                    upperIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    substances,
                    isPerPerson
                );
                NRecords = upperIntakes.Select(c => c.SimulatedIndividual.Id).Distinct().Count();
                if (NRecords > 0) {
                    var oims = upperIntakes
                        .GroupBy(c => c.SimulatedIndividual.Id)
                        .Select(c => c.Average(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }

            CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) / dietaryIndividualDayIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            setUncertaintyBounds();
        }

        /// <summary>
        /// The number of Food, Substance and Processing combinations can be very large. Furthermore, in the bootstrap new combinations may pop up
        /// or combinations that are present in the nominal run do not pop up. Therefor, the mechanism is based on the number of bootstraps. In each bootstrap run,
        /// zero contributions are added when a combination is not present in the bootstrap or nominal run.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="substances"></param>
        /// <param name="exposureType"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);

            UncertaintyCycles++;
            if (exposureType == ExposureType.Acute) {
                var records = summarizeAcute(
                    upperIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    substances,
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
