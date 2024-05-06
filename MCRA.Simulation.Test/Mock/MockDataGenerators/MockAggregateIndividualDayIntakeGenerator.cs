using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock aggregate individual day exposures.
    /// </summary>
    public static class MockAggregateIndividualDayIntakeGenerator {

        /// <summary>
        /// Creates aggregate individual day exposures.
        /// </summary>
        /// <returns></returns>
        public static List<AggregateIndividualDayExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            ITargetExposuresCalculator targetExposuresCalculator,
            ExposureUnitTriple externalExposuresUnit,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var foods = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(simulatedIndividualDays, foods, substances, 0.5, true, random);
            var nonDietaryExposureRoutes = exposureRoutes
                .Where(r => r != ExposurePathType.Dietary)
                .ToList();
            var nonDietaryIndividualDayIntakes = MockNonDietaryIndividualDayIntakeGenerator
                .Generate(simulatedIndividualDays, substances, nonDietaryExposureRoutes, 0.5, random);
            var aggregateIndividualDayExposures = AggregateIntakeCalculator
                .CreateAggregateIndividualDayExposures(
                    dietaryIndividualDayIntakes,
                    nonDietaryIndividualDayIntakes,
                    exposureRoutes
                );
            if (targetExposuresCalculator != null) {
                var targetIndividualDayExposures = targetExposuresCalculator
                    .ComputeTargetIndividualDayExposures(
                        aggregateIndividualDayExposures.Cast<IExternalIndividualDayExposure>().ToList(),
                        substances,
                        substances.First(),
                        exposureRoutes,
                        externalExposuresUnit,
                        new List<TargetUnit>() { targetUnit },
                        random,
                        new ProgressState()
                    );
                var lookup = targetIndividualDayExposures.First()
                    .TargetIndividualDayExposures
                    .ToDictionary(c => c.SimulatedIndividualDayId, c => c.TargetExposuresBySubstance);
                foreach (var record in aggregateIndividualDayExposures) {
                    record.TargetExposuresBySubstance = lookup[record.SimulatedIndividualDayId];
                }
            }
            return aggregateIndividualDayExposures;
        }
    }
}
