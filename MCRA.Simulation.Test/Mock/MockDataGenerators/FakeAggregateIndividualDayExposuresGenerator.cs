using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock aggregate individual day exposures.
    /// </summary>
    public static class FakeAggregateIndividualDayExposuresGenerator {

        public static List<AggregateIndividualDayExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<TargetUnit> targetUnits,
            IRandom random,
            List<double> mu = null,
            List<double> sigma = null,
            double fractionZeros = 0
        ) {
            mu = mu ?? substances.Select(r => 0D).ToList();
            sigma = sigma ?? substances.Select(r => 1D).ToList();
            var result = simulatedIndividualDays
                .Select(r => new AggregateIndividualDayExposure() {
                    SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    IndividualSamplingWeight = r.IndividualSamplingWeight,
                    Day = r.Day,
                    Individual = r.Individual,
                    InternalTargetExposures = targetUnits
                        .ToDictionary(
                            r => r.Target,
                            r => substances
                                .Select((c, ix) => new SubstanceTargetExposure(c, random.NextDouble() > fractionZeros ? LogNormalDistribution.Draw(random, mu[ix], sigma[ix]) : 0D))
                                .ToDictionary(c => c.Substance, c => c as ISubstanceTargetExposure)
                        )
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates aggregate individual day exposures.
        /// </summary>
        public static List<AggregateIndividualDayExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var exposureRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var result = Create(
                simulatedIndividualDays,
                substances,
                exposureRoutes,
                kineticModelCalculators,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                targetUnit,
                random
            );
            return result;
        }

        /// <summary>
        /// Creates aggregate individual day exposures.
        /// </summary>
        public static List<AggregateIndividualDayExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators,
            ExposureUnitTriple externalExposuresUnit,
            TargetUnit targetUnit,
            IRandom random
        ) {
            // TO DO: replace null by mock dust exposure data
            var foods = MockFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = MockDietaryIndividualDayIntakeGenerator
                .Create(simulatedIndividualDays, foods, substances, 0.5, true, random);
            var nonDietaryExposureRoutes = exposureRoutes
                .Where(r => r != ExposurePathType.Oral)
                .ToList();
            var nonDietaryIndividualDayIntakes = MockNonDietaryIndividualDayIntakeGenerator
                .Generate(simulatedIndividualDays, substances, nonDietaryExposureRoutes, 0.5, random);
            var aggregateIndividualDayExposures = AggregateIntakeCalculator
                .CreateCombinedIndividualDayExposures(
                    dietaryIndividualDayIntakes,
                    nonDietaryIndividualDayIntakes,
                    null,
                    exposureRoutes
                );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(
                kineticModelCalculators
            );
            var targetIndividualDayExposures = internalTargetExposuresCalculator
                .ComputeAcute(
                    aggregateIndividualDayExposures,
                    substances,
                    exposureRoutes,
                    externalExposuresUnit,
                    [targetUnit],
                    random,
                    new ProgressState()
                );

            return targetIndividualDayExposures.ToList();
        }
    }
}
