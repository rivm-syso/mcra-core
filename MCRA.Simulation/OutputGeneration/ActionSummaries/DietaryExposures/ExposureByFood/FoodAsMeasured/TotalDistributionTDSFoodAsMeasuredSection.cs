using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes for modelled foods the relative contribution to the upper tail of the exposure distribution and other statistics.
    /// </summary>
    public sealed class TotalDistributionTDSFoodAsMeasuredSection : DistributionTDSFoodAsMeasuredSectionBase {

        public List<TDSReadAcrossFoodRecord> Records { get; set; }

        [Obsolete]
        public string Exposure { get; set; }

        public void Summarize(
                ICollection<DietaryIndividualDayIntake> intakes,
                ICollection<Compound> selectedCompounds,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                ExposureType exposureType,
                double lowerPercentage,
                double upperPercentage,
                bool isPerPerson
            ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(intakes, relativePotencyFactors, membershipProbabilities, selectedCompounds, isPerPerson);
            } else {
                Records = SummarizeChronic(intakes, relativePotencyFactors, membershipProbabilities, selectedCompounds, isPerPerson);
            }
        }
    }
}
