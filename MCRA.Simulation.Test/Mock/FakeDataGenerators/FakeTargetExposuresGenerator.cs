﻿using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.ComponentCalculation.Component;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock target exposures
    /// </summary>
    public static class FakeTargetExposuresGenerator {

        /// <summary>
        /// Creates a list of target individual day exposures
        /// </summary>
        public static List<ITargetIndividualDayExposure> MockIndividualDayExposures(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            IRandom random,
            List<double> mu = null,
            List<double> sigma = null,
            double fractionZeros = 0
        ) {
            mu = mu ?? substances.Select(r => 0D).ToList();
            sigma = sigma ?? substances.Select(r => 1D).ToList();
            var result = simulatedIndividualDays
                .Select(r => new TargetIndividualDayExposure() {
                    SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                    Day = r.Day,
                    SimulatedIndividual = r.SimulatedIndividual,
                    TargetExposuresBySubstance = substances
                        .Select((c, ix) => new SubstanceTargetExposure(c, random.NextDouble() > fractionZeros ? LogNormalDistribution.Draw(random, mu[ix], sigma[ix]) : 0D))
                        .ToDictionary(c => c.Substance, c => c as ISubstanceTargetExposure)
                })
                .Cast<ITargetIndividualDayExposure>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates a list of target individual exposures
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fractionZeros"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<ITargetIndividualExposure> MockIndividualExposures(
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            IRandom random,
            List<double> mu = null,
            List<double> sigma = null,
            double fractionZeros = 0
        ) {
            mu = mu ?? substances.Select(r => 0D).ToList();
            sigma = sigma ?? substances.Select(r => 1D).ToList();
            var result = individuals
                .Select((r, ix) => new TargetIndividualExposure() {
                    SimulatedIndividual = new(r, ix),
                    TargetExposuresBySubstance = substances
                        .Select((c, ixs) => new SubstanceTargetExposure(c, random.NextDouble() > fractionZeros ? LogNormalDistribution.Draw(random, mu[ixs], sigma[ixs]) : 0D))
                        .ToDictionary(c => c.Substance, c => c as ISubstanceTargetExposure)
                })
                .Cast<ITargetIndividualExposure>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates a list of target individual day exposures
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="substances"></param>
        /// <param name="components"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<ITargetIndividualDayExposure> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<Component> components,
            IRandom random
        ) {
            var result = simulatedIndividualDays
                .Select(r => new TargetIndividualDayExposure() {
                    SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                    Day = r.Day,
                    SimulatedIndividual = r.SimulatedIndividual,
                    TargetExposuresBySubstance = createSubstanceTargetExposures(substances, components.DrawRandom(random, p => p.Fraction), random)
                })
                .Cast<ITargetIndividualDayExposure>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates a dictionary of target individual day exposures
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="component"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static IDictionary<Compound, ISubstanceTargetExposure> createSubstanceTargetExposures(
            ICollection<Compound> substances,
            Component component,
            IRandom random
        ) {
            return substances
                .Select(r => component.ComponentRecords.TryGetValue(r, out var record)
                    ? new SubstanceTargetExposure(r, LogNormalDistribution.Draw(random, record.Mu, record.Sigma))
                    : new SubstanceTargetExposure(r, 0D)
                )
                .ToDictionary(r => r.Substance, r => r as ISubstanceTargetExposure);
        }
    }
}
