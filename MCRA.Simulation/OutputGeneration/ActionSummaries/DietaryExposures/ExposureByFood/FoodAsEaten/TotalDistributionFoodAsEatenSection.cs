using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    /// <summary>
    /// Summarizes for foods as eaten the relative contribution to the total exposure distribution and other statistics.
    /// </summary>
    public sealed class TotalDistributionFoodAsEatenSection : DistributionFoodAsEatenSectionBase {

        [Obsolete]
        public string Exposure { get; set; }

        [Obsolete]
        public string Note { get; set; }

        public UncertainDataPointCollection<string> Contribution {
            get {
                return _contribution;
            }
        }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            Percentages = [lowerPercentage, 50, upperPercentage];
            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
            } else {
                Records = SummarizeChronic(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
            }
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
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                var distributionFoodAsEatenRecords = SummarizeUncertaintyAcute(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
                updateContributions(distributionFoodAsEatenRecords);
            } else {
                var distributionFoodAsEatenRecords = SummarizeUncertaintyChronic(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
                updateContributions(distributionFoodAsEatenRecords);
            }
        }

        public void SummarizeUncertaintyResults(List<DistributionFoodRecord> tdFoodsAsEaten) {
            _contribution.AddUncertaintyValues(tdFoodsAsEaten.Select(t => t.Contribution));
        }
    }
}
