using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public class AggregateTotalIntakeDistributionSection : AggregateDistributionSectionBase, IIntakeDistributionSection {
        public override bool SaveTemporaryData => true;
        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute). Make sure to call SummarizeReferenceResults first. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="intakes">The resampled set of intakes</param>
        public void SummarizeUncertainty(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var weights = aggregateIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            _percentiles.AddUncertaintyValues(aggregateIndividualDayExposures.Select(i => i.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).PercentilesWithSamplingWeights(weights, _percentiles.XValues));
        }
    }
}
