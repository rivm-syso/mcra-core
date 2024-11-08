using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the upper tail of the transformed non-dietary exposure distribution in bins, is used for plotting of the transformed exposure distribution
    /// </summary>
    [XmlInclude(typeof(DietaryUpperIntakeCoExposureDistributionSection))]
    public class DietaryUpperIntakeDistributionSection : DietaryDistributionSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var dietaryIntakes = dietaryIndividualDayIntakes
                .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = dietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var intakeValue = dietaryIntakes.PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var upperIntakes = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)  > intakeValue)
                .ToList();
            if (upperIntakes.Count == 0) {
                upperIntakes = dietaryIndividualDayIntakes
                    .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) >= intakeValue)
                    .ToList();
            }
            CalculatedUpperPercentage =  upperIntakes.Sum(c => c.IndividualSamplingWeight)
                / dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
            var dietaryUpperIntakes = upperIntakes
                .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .DefaultIfEmpty(double.NaN)
                .ToList();
            LowPercentileValue = dietaryUpperIntakes.Min();
            HighPercentileValue = dietaryUpperIntakes.Max();
            NRecords = upperIntakes.Count;
            Summarize(
                null,
                upperIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                null,
                isPerPerson,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
        }
    }
}
