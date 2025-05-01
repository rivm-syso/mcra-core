using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

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
                    SimulatedIndividual = r.SimulatedIndividual,
                    SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                    Day = r.Day,
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
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var result = Create(
                simulatedIndividualDays,
                substances,
                routes,
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
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators,
            ExposureUnitTriple externalExposuresUnit,
            TargetUnit targetUnit,
            IRandom random
        ) {
            // TODO: add mock dust exposure data
            var foods = FakeFoodsGenerator.Create(3);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator
                .Create(simulatedIndividualDays, foods, substances, 0.5, true, random);
            var nonDietaryExposureRoutes = routes
                .Where(r => r != ExposureRoute.Oral)
                .ToList();
            var nonDietaryIndividualDayIntakes = FakeNonDietaryIndividualDayIntakeGenerator
                .Generate(simulatedIndividualDays, substances, nonDietaryExposureRoutes, 0.5, random);

            var nonDietaryExternalIndividualDayExposures = nonDietaryIndividualDayIntakes
                .Select(r => r as IExternalIndividualDayExposure)
                .ToList();

            var externalExposureCollections = new List<ExternalExposureCollection>();
            if (nonDietaryExternalIndividualDayExposures?.Count > 0) {
                var nonDietaryExposureCollection = new ExternalExposureCollection {
                    ExposureSource = ExposureSource.OtherNonDiet,
                    SubstanceAmountUnit = externalExposuresUnit.SubstanceAmountUnit,
                    ExternalIndividualDayExposures = nonDietaryExternalIndividualDayExposures
                };
                externalExposureCollections.Add(nonDietaryExposureCollection);
            }

            var aggregateIndividualDayExposures = CombinedExternalExposuresCalculator
                .CreateCombinedIndividualDayExposures(
                    externalExposureCollections,
                    externalExposuresUnit,
                    ExposureType.Acute,
                    new CancellationToken()
                );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(
                kineticModelCalculators
            );
            var targetIndividualDayExposures = internalTargetExposuresCalculator
                .ComputeAcute(
                    aggregateIndividualDayExposures,
                    substances,
                    routes,
                    externalExposuresUnit,
                    [targetUnit],
                    random,
                    new ProgressState()
                );

            return targetIndividualDayExposures.ToList();
        }
    }
}
