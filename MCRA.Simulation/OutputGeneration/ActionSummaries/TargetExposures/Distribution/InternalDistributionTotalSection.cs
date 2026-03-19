using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
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
            PopulationStratifier populationStratifier,
            TargetUnit targetUnit
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors
                    ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities
                    ?? substances.ToDictionary(r => r, r => 1D);
            }

            // Total distribution section: histogram and cumulative distribution chart
            SummarizeUnstratifiedBinsGraph(
                aggregateIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                populationStratifier,
                targetUnit
            );

            SummarizeStratifiedBinsGraph(
                aggregateIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                populationStratifier,
                targetUnit
            );

            //TODO: to discuss with Jasper, move this section to exposures by route
            //SummarizeCategorizedBins(
            //    aggregateIndividualExposures,
            //    relativePotencyFactors,
            //    membershipProbabilities,
            //    routes, 
            //    kineticConversionFactors,
            //    externalExposureUnit,
            //    targetUnit
            //);
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
            ICollection<AggregateIndividualExposure> aggregateExposures,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            TargetUnit targetUnit,
            PopulationStratifier populationStratifier,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            {
            var exposures = aggregateExposures
                    .Select(c => (
                        Exposure: c.GetTotalExposureAtTarget(
                            targetUnit.Target,
                            rpfs,
                            memberships
                        ),
                        SimulatedIndividual: c.SimulatedIndividual
                    ));

            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            _percentiles.AddUncertaintyValues(exposures.Select(c => c.Exposure).PercentilesWithSamplingWeights(weights, [.. _percentiles.XValues]));
                }
            if (populationStratifier != null) {
                var exposures = aggregateExposures
                    .Select(c => (
                        Exposure: c.GetTotalExposureAtTarget(
                            targetUnit.Target,
                            rpfs,
                            memberships
                        ),
                        SimulatedIndividual: c.SimulatedIndividual,
                        StratificationLevel: populationStratifier.GetLevel(c.SimulatedIndividual).Code
                    ))
                    .GroupBy(c => c.StratificationLevel)
                    .ToList();
                foreach (var group in exposures) {
                    var weights = group
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var collection = StratifiedPercentiles.Where(c => c.Item1 == group.Key).FirstOrDefault().Item2;
                    if (collection != null) {
                        var percentiles = group
                            .Select(c => c.Exposure)
                            .PercentilesWithSamplingWeights(weights, [.. collection.XValues]);
                        collection.AddUncertaintyValues(percentiles);
                    }
                }
            } 
                        
        }
    }
}
