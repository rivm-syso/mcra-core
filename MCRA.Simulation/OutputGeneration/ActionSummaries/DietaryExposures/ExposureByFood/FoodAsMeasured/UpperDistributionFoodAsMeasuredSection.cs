using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes for modelled foods the relative contribution to the upper tail of the distribution and other statistics.
    /// </summary>
    public sealed class UpperDistributionFoodAsMeasuredSection : DistributionFoodAsMeasuredSectionBase {

        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public int NRecords { get; set; }

        [Obsolete]
        public string Exposure { get; set; }

        [Obsolete]
        public string Note { get; set; }

        public void Summarize(
            ICollection<Food> allFoods,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Food> modelledFoods,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);

            if (exposureType == ExposureType.Acute) {
                SummarizeAcute(
                    allFoods,
                    modelledFoods,
                    upperIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
            } else {
                SummarizeChronic(
                    allFoods,
                    modelledFoods,
                    upperIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
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
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private void setUncertaintyBounds(double uncertaintyLowerBound, double uncertaintyUpperBound) {
            foreach (var item in Records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
            foreach (var item in HierarchicalNodes) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        public void SummarizeUncertainty(
            ICollection<Food> allFoods,
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
                SummarizeUncertaintyAcute(
                    allFoods, 
                    upperIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    isPerPerson
                );
            } else {
                SummarizeUncertaintyChronic(
                    allFoods,
                    upperIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    isPerPerson
                );
            }
        }
    }
}
