using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes for foods as eaten the relative contribution to the upper tail of the distribution and other statistics.
    /// </summary>
    public sealed class UpperDistributionFoodAsEatenSection : DistributionFoodAsEatenSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        [Obsolete]
        public string Note { get; set; }

        [Obsolete]
        public string Exposure { get; set; }

        public void Summarize(
                ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                ICollection<Food> foods,
                ExposureType exposureType,
                double lowerPercentage,
                double upperPercentage,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound,
                double percentageForUpperTail,
                bool isPerPerson
            ) {
            Percentages = [lowerPercentage, 50, upperPercentage];
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
                Records = SummarizeAcute(upperIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    var dietaryUpperIntakes = upperIntakes
                        .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();
                    LowPercentileValue = dietaryUpperIntakes.Min();
                    HighPercentileValue = dietaryUpperIntakes.Max();
                }
            } else {
                Records = SummarizeChronic(upperIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
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

            var foodCodes = Records.Select(c => c.FoodCode).ToHashSet();
            foreach (var food in foods) {
                if (!foodCodes.Contains(food.Code)) {
                    Records.Add(new DistributionFoodRecord() {
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        Contributions = [],
                    }); ;
                }
            }
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) / dietaryIndividualDayIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private void setUncertaintyBounds(double uncertaintyLowerBound, double uncertaintyUpperBound) {
            foreach (var item in Records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);

            if (exposureType == ExposureType.Acute) {
                var distributionFoodAsEatenRecords = SummarizeUncertaintyAcute(upperIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
                updateContributions(distributionFoodAsEatenRecords);
            } else {
                var distributionFoodAsEatenRecords = SummarizeUncertaintyChronic(upperIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
                updateContributions(distributionFoodAsEatenRecords);
            }
        }
    }
}
