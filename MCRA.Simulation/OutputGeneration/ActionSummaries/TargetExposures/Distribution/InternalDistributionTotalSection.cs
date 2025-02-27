using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Stores the total transformed exposure distribution in bins, is used for plotting the transformed exposure distribution
    /// </summary>
    public class InternalDistributionTotalSection : InternalDistributionSectionBase, IIntakeDistributionSection {

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors
                    ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities
                    ?? substances.ToDictionary(r => r, r => 1D);
            }

            var aggregates = aggregateIndividualExposures
                .Select(c => (
                    Exposure: c.GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    ),
                    SamplingWeight: c.IndividualSamplingWeight
                ))
                .ToList();

            // Total distribution section
            //var totalDistributionSection = new InternalDistributionTotalSection();
            Summarize(aggregates);
            SummarizeCategorizedBins(
                aggregateIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                routes,
                kineticConversionFactors,
                externalExposureUnit,
                targetUnit
            );
        }


        public void Summarize(
            List<int> coExposureIds,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<ExposureRoute> routes,
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
                routes,
                externalExposureUnit,
                targetUnit,
                percentages,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
        }

        public void SummarizeUncertainty(
            List<double> intakes,
            List<double> weights,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            _percentiles.AddUncertaintyValues(intakes.PercentilesWithSamplingWeights(weights, _percentiles.XValues.ToArray()));
        }
    }
}
