using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public class AggregateTotalIntakeDistributionSection : AggregateDistributionSectionBase, IIntakeDistributionSection {

        public override bool SaveTemporaryData => true;

        public void Summarize(
            List<int> coExposureIds,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            double[] percentages,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            summarize(
                coExposureIds,
                aggregateIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                exposureRoutes,
                externalExposureUnit,
                targetUnit,
                percentages,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle (acute). Make sure to call SummarizeReferenceResults first. Percentiles (output) from specified percentages (input).
        /// </summary>
        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureTarget target
        ) {
            var weights = aggregateIndividualExposures
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var uncertaintyValues = aggregateIndividualExposures
                .Select(i => i.GetTotalExposureAtTarget(
                    target,
                    relativePotencyFactors,
                    membershipProbabilities
                ))
                .PercentilesWithSamplingWeights(weights, _percentiles.XValues);
            _percentiles.AddUncertaintyValues(uncertaintyValues);
        }
    }
}
