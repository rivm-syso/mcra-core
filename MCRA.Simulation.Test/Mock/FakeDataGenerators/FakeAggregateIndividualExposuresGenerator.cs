using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
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
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fractionZeros"></param>
        /// <returns></returns>
        public static List<AggregateIndividualExposure> Create(
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
                .Select(r => new AggregateIndividualExposure() {
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    IndividualSamplingWeight = r.IndividualSamplingWeight,
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

        public static List<AggregateIndividualExposure> Create(
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
        /// Creates fake aggregate individual exposures.
        /// </summary>
        public static List<AggregateIndividualExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var externalIndividualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(
                    simulatedIndividualDays,
                    substances,
                    routes,
                    random.Next()
                );
            var aggregateIndividualExposures = AggregateIntakeCalculator
                .CreateCombinedExternalIndividualExposures(externalIndividualDayExposures);

            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(
                kineticModelCalculators
            );
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

            return result.ToList();
        }
    }
}
