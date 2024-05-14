using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes for modelled foods the relative contribution to the upper tail of the exposure distribution and other statistics.
    /// </summary>
    public sealed class UpperDistributionTDSFoodAsMeasuredSection : DistributionTDSFoodAsMeasuredSectionBase {

        public List<TDSReadAcrossFoodRecord> Records { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

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
                double percentageForUpperTail,
                bool isPerPerson
            ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(
                    intakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    percentageForUpperTail, 
                    isPerPerson
                );

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(upperIntakes, relativePotencyFactors, membershipProbabilities, selectedCompounds, isPerPerson);
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    var dietaryUpperIntakes = upperIntakes
                        .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();  
                    LowPercentileValue = dietaryUpperIntakes.Min();
                    HighPercentileValue = dietaryUpperIntakes.Max();
                }
            } else {
                Records = SummarizeChronic(upperIntakes, relativePotencyFactors, membershipProbabilities, selectedCompounds, isPerPerson);
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
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / intakes.Sum(c => c.IndividualSamplingWeight) * 100;
        }
    }
}

