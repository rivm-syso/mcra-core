using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock aggregate individual exposures
    /// </summary>
    public static class FakeAggregateIndividualExposuresGenerator {

        /// <summary>
        /// Creates a list of target individual day exposures
        /// </summary>
        public static List<AggregateIndividualExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<TargetUnit> targetUnits,
            IRandom random,
            List<double> mu = null,
            List<double> sigma = null,
            double fractionZeros = 0
        ) {
            mu = mu ?? [.. substances.Select(r => 0D)];
            sigma = sigma ?? [.. substances.Select(r => 1D)];
            var result = simulatedIndividualDays
                .Select(r => new AggregateIndividualExposure() {
                    SimulatedIndividual = r.SimulatedIndividual,
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

        public static List<AggregateIndividualExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var paths = FakeExposurePathGenerator
                .Create(ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var result = Create(
                simulatedIndividualDays,
                substances,
                paths,
                kineticModelCalculators,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                targetUnit,
                random
            );
            return result;
        }

        /// <summary>
        /// Creates fake aggregate individual exposures.
        /// </summary>
        public static List<AggregateIndividualExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePath> paths,
            IDictionary<Compound, IKineticConversionCalculator> kineticModelCalculators,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var externalIndividualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(
                    simulatedIndividualDays,
                    substances,
                    paths,
                    random.Next()
                );
            var routes = paths.Select(p => p.Route).ToHashSet();
            var aggregateIndividualExposures = CombinedExternalExposuresCalculator
                .CreateCombinedExternalIndividualExposures(
                externalIndividualDayExposures,
                routes
            );

            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(kineticModelCalculators);
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);

            var result = internalTargetExposuresCalculator
                .ComputeChronic(
                    aggregateIndividualExposures,
                    substances,
                    routes,
                    exposureUnit,
                    [targetUnit],
                    random,
                    new ProgressState()
                );

            return [.. result];
        }
    }
}
