using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed external exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public sealed class ExternalTotalExposureDistributionSection : ExternalExposureDistributionBase, IIntakeDistributionSection {

        /// <summary>
        /// Summarizes this section based on the main simulation run.
        /// </summary>
        /// <param name="coExposures"></param>
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="percentages"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="uncertaintyLowerLimit"></param>
        /// <param name="uncertaintyUpperLimit"></param>
        /// 
        public void Summarize(
            HashSet<int> coExposures,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double[] percentages,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            base.Summarize(
                coExposures,
                externalIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                percentages,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit,
                isPerPerson);
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute). ercentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        ///
        public void SummarizeUncertainty(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var weights = externalIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            _percentiles.AddUncertaintyValues(externalIndividualDayExposures
                .Select(i => i.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .PercentilesWithSamplingWeights(weights, _percentiles.XValues));
        }
    }
}
