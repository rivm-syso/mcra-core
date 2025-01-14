using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionCompoundSection : DistributionCompoundSectionBase {
        public double? UpperPercentage { get; set; } = null;
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
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
                Records = SummarizeDietaryAcute(
                    upperIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes.Count;
                if (NumberOfIntakes > 0) {
                    var aggregateUpperIntakes = upperIntakes
                        .Select(c => c.TotalExposurePerMassUnit(
                            relativePotencyFactors,
                            membershipProbabilities,
                            isPerPerson
                        ))
                        .ToList();
                    LowPercentileValue = aggregateUpperIntakes.Min();
                    HighPercentileValue = aggregateUpperIntakes.Max();
                }
            } else {
                Records = SummarizeDietaryChronic(
                    upperIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes
                    .Select(c => c.SimulatedIndividual.Id)
                    .Distinct()
                    .Count();
                if (NumberOfIntakes > 0) {
                    var oims = upperIntakes
                        .GroupBy(c => c.SimulatedIndividual.Id)
                        .Select(c => c.Average(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }

            var substanceCodes = Records.Select(c => c.CompoundCode).ToHashSet();
            foreach (var substance in substances) {
                if (!substanceCodes.Contains(substance.Code)) {
                    Records.Add(new DistributionCompoundRecord() {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                    });
                }
            }
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) / dietaryIndividualDayIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            SetUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeUncertaintyAcute(upperIntakes, substances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                UpdateContributions(records);
            } else {
                var records = SummarizeUncertaintyChronic(upperIntakes, substances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                UpdateContributions(records);
            }
        }
    }
}
