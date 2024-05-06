using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock aggregate individual exposures
    /// </summary>
    public static class MockAggregateIndividualIntakeGenerator {

        /// <summary>
        /// Creates  aggregate individual exposures
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="substances"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="kineticModelCalculators"></param>
        /// <param name="targetUnit"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<AggregateIndividualExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var aggregateIndividualDayExposures = MockAggregateIndividualDayIntakeGenerator
                .Create(
                    simulatedIndividualDays,
                    substances,
                    exposureRoutes,
                    null,
                    exposureUnit,
                    targetUnit,
                    random
                );
            var aggregateIndividualExposures = AggregateIntakeCalculator
                .CreateAggregateIndividualExposures(aggregateIndividualDayExposures);

            var progressReport = new CompositeProgressState();

            var relativeCompartmentWeight = kineticModelCalculators[substances.First()].GetNominalRelativeCompartmentWeight().ToDictionary(c => c.Item1, c => c.Item2);
            foreach (var substance in substances) {
                var calculator = kineticModelCalculators[substance];
                var individualExposures = aggregateIndividualExposures.Cast<IExternalIndividualExposure>().ToList();
                var substanceIndividualTargetExposures = calculator
                    .CalculateIndividualTargetExposures(
                        individualExposures,
                        substance,
                        exposureRoutes,
                        exposureUnit,
                        relativeCompartmentWeight,
                        new ProgressState(progressReport.CancellationToken),
                        random
                    );
                var substanceIndividualTargetExposuresLookup = substanceIndividualTargetExposures
                    .First().IndividualSubstanceTargetExposures
                    .ToDictionary(r => r.SimulatedIndividualId, r => r.SubstanceTargetExposures);
                foreach (var aggregateIndividualExposure in aggregateIndividualExposures) {
                    aggregateIndividualExposure.TargetExposuresBySubstance.Add(substance, substanceIndividualTargetExposuresLookup[aggregateIndividualExposure.SimulatedIndividualId].First());
                }
            }
            return aggregateIndividualExposures;
        }
    }
}
