using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total untransformed exposure distribution in bins,
    /// is used for plotting of the untransformed exposure distribution.
    /// </summary>
    public class InternalUntransformedExposureDistributionSection : SummarySection {

        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public double PercentageZeroIntake { get; set; }

                /// <summary>
        /// Summarizes aggregate individual day exposures.
        /// </summary>
        public void Summarize(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureTarget target
        ) {
            var numberOfIntakes = aggregateIndividualDayIntakes.Count;
            var positiveDayIntakes = aggregateIndividualDayIntakes
                .Select(c =>
                    (Exposure: c.GetTotalExposureAtTarget(
                            target,
                            relativePotencyFactors,
                            membershipProbabilities
                        ),
                     SamplingWeight: c.SimulatedIndividual.SamplingWeight
                ))
                .Where(v => v.Exposure > 0)
                .ToList();
            if (positiveDayIntakes.Any()) {
                var weights = positiveDayIntakes
                    .Select(id => id.SamplingWeight)
                    .ToList();
                var exposures = positiveDayIntakes
                    .Select(c => c.Exposure)
                    .ToList();
                var min = exposures.Min();
                var max = exposures.Max();
                int numberOfBins = Math.Sqrt(numberOfIntakes) < 100 ? BMath.Ceiling(Math.Sqrt(numberOfIntakes)) : 100;
                IntakeDistributionBins = exposures.MakeHistogramBins(weights, numberOfBins, min, max);
                PercentageZeroIntake = (numberOfIntakes - positiveDayIntakes.Count) / (double)numberOfIntakes * 100;
            } else {
                IntakeDistributionBins = null;
                PercentageZeroIntake = 100;
            }
        }
    }
}