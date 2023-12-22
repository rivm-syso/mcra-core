using MCRA.Utils;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total untransformed exposure distribution in bins,
    /// is used for plotting of the untransformed exposure distribution.
    /// </summary>
    public class UntransformedTotalIntakeDistributionSection : ActionSummarySectionBase {
        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public int TotalNumberOfIntakes { get; set; }
        public double PercentageZeroIntake { get; set; }

        /// <summary>
        /// Summarizes dietary individual day exposures
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            TotalNumberOfIntakes = dietaryIndividualDayIntakes.Count;
            var positives = dietaryIndividualDayIntakes
                .Where(v => v.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .ToList();
            if (positives.Any()) {
                var exposures = positives
                    .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                    .ToList();
                var weights = positives
                    .Select(id => id.IndividualSamplingWeight)
                    .ToList();
                var min = exposures.Min();
                var max = exposures.Max();
                int numberOfBins = Math.Sqrt(TotalNumberOfIntakes) < 100 ? BMath.Ceiling(Math.Sqrt(TotalNumberOfIntakes)) : 100;
                IntakeDistributionBins = exposures.MakeHistogramBins(weights, numberOfBins, min, max);
                PercentageZeroIntake = dietaryIndividualDayIntakes.Count(c => c.TotalExposure(relativePotencyFactors, membershipProbabilities) == 0) / (double)TotalNumberOfIntakes * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100;
            }
        }

        /// <summary>
        /// Summarizes aggregate individual day exposures.
        /// </summary>
        /// <param name="aggregateIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            TotalNumberOfIntakes = aggregateIndividualDayIntakes.Count;
            var positives = aggregateIndividualDayIntakes
                .Where(v => v.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                .ToList();
            if (positives.Any()) {
                var exposures = positives
                    .Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson))
                    .ToList();
                var weights = aggregateIndividualDayIntakes
                    .Where(v => v.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                    .Select(id => id.IndividualSamplingWeight)
                    .ToList();
                var min = exposures.Min();
                var max = exposures.Max();
                int numberOfBins = Math.Sqrt(TotalNumberOfIntakes) < 100 ? BMath.Ceiling(Math.Sqrt(TotalNumberOfIntakes)) : 100;
                IntakeDistributionBins = exposures.MakeHistogramBins(weights, numberOfBins, min, max);
                PercentageZeroIntake = aggregateIndividualDayIntakes.Count(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) == 0) / (double)TotalNumberOfIntakes * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100;
            }
        }
    }
}
