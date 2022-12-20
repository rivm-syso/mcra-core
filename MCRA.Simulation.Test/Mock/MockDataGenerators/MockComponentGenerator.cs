using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.Component;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock components
    /// </summary>
    public static class MockComponentGenerator {
        /// <summary>
        /// Creates a list of components
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="numberOfComponents"></param>
        /// <param name="numberOfZeroExposureSubstances"></param>
        /// <param name="sparseness"></param>
        /// <param name="sigma"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static ICollection<Component> CreateComponents(
            List<Compound> substances,
            IRandom random,
            int numberOfComponents = 4,
            int numberOfZeroExposureSubstances = 0,
            double sparseness = 0.5,
            double sigma = 1D
        ) {

            if (numberOfZeroExposureSubstances > substances.Count) {
                throw new Exception("Number of zero exposure substances should be smaller than the number of substances.");
            }
            if (numberOfComponents > substances.Count - numberOfZeroExposureSubstances) {
                throw new Exception("Number of components should be smaller than the number of substances.");
            }

            var isInComponent = new bool[numberOfComponents, substances.Count];
            for (int i = 0; i < numberOfComponents; i++) {
                for (int j = 0; j < substances.Count - numberOfZeroExposureSubstances; j++) {
                    isInComponent[i, j] = random.NextDouble() < sparseness ? true : false;
                }
                for (int j = substances.Count - numberOfZeroExposureSubstances; j < substances.Count; j++) {
                    isInComponent[i, j] = false;
                }
            }

            var result = new List<Component>();
            for (int i = 0; i < numberOfComponents; i++) {
                var componentSubstances = substances.Where((r, ix) => isInComponent[i, ix]).ToList();
                var normal = new NormalDistribution(0, sigma);
                var mus = normal.Draws(random, componentSubstances.Count).ToArray();
                var logNormal = new LogNormalDistribution(0, 1);
                var sigmas = logNormal.Draws(random, componentSubstances.Count).ToArray();
                result.Add(CreateComponent(componentSubstances, mus, sigmas, 1D / numberOfComponents));
            }
            return result;
        }
        /// <summary>
        /// Creates a component
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public static Component CreateComponent(List<Compound> substances, double[] mu, double[] sigma, double fraction) {
            return new Component() {
                Fraction = fraction,
                ComponentRecords = substances
                    .Select((r, ix) => new ComponentRecord() {
                        Compound = r,
                        Mu = mu[ix],
                        Sigma = sigma[ix]
                    })
                    .ToDictionary(r => r.Compound),
            };
        }
        /// <summary>
        /// Creates an exposure matrix based on components
        /// </summary>
        /// <param name="recordsIds"></param>
        /// <param name="substances"></param>
        /// <param name="numberOfComponents"></param>
        /// <param name="numberOfZeroExposureSubstances"></param>
        /// <param name="numberOfZeroExposureRecords"></param>
        /// <param name="sigma"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ExposureMatrix CreateExposureMatrix(
            List<int> recordsIds,
            List<Compound> substances,
            int numberOfComponents = 4,
            int numberOfZeroExposureSubstances = 0,
            int numberOfZeroExposureRecords = 0,
            double sigma = 1D,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);

            if (numberOfZeroExposureSubstances > substances.Count) {
                throw new Exception("Number of zero exposure substances should be smaller than the number of substances.");
            }
            if (numberOfComponents > substances.Count - numberOfZeroExposureSubstances) {
                throw new Exception("Number of components should be smaller than the number of substances.");
            }

            var components = new double[numberOfComponents, substances.Count];
            for (int i = 0; i < numberOfComponents; i++) {
                for (int j = 0; j < substances.Count - numberOfZeroExposureSubstances; j++) {
                    var logNormal = new LogNormalDistribution(0, 1);
                    components[i, j] = j % numberOfComponents == i ? logNormal.Draw(random) : 0D;
                }
                for (int j = substances.Count - numberOfZeroExposureSubstances; j < substances.Count; j++) {
                    components[i, j] = 0D;
                }
            }

            var exposures = new double[substances.Count, recordsIds.Count];
            for (int i = 0; i < recordsIds.Count - numberOfZeroExposureRecords; i++) {
                var idComponent = i % numberOfComponents;
                for (int j = 0; j < substances.Count; j++) {
                    if (components[idComponent, j] > 0D) {
                        var logNormal = new LogNormalDistribution(0, sigma);
                        var noise = logNormal.Draw(random);
                        exposures[j, i] = components[idComponent, j] + noise;
                    } else {
                        exposures[j, i] = 0D;
                    }
                }
            }
            for (int i = recordsIds.Count - numberOfZeroExposureRecords; i < recordsIds.Count; i++) {
                for (int j = 0; j < substances.Count; j++) {
                    exposures[j, i] = 0D;
                }
            }

            return new ExposureMatrix() {
                Exposures = new GeneralMatrix(exposures),
                Substances = substances,
                Individuals = recordsIds.Select(c => new Individual(c) { Code = c.ToString()}).ToList(),
                Sds = Enumerable.Repeat(1d, substances.Count).ToList()
            };
        }

        /// <summary>
        /// Creates IndividualExposureMatrix
        /// </summary>
        /// <param name="recordsIds"></param>
        /// <param name="substances"></param>
        /// <param name="numberOfComponents"></param>
        /// <param name="numberOfZeroExposureSubstances"></param>
        /// <param name="numberOfZeroExposureRecords"></param>
        /// <param name="sigma"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IndividualMatrix CreateIndividualMatrix(
            List<int> recordsIds,
            List<Compound> substances,
            int numberOfComponents = 4,
            int numberOfZeroExposureSubstances = 0,
            int numberOfZeroExposureRecords = 0,
            double sigma = 1D,
            int seed = 1
        ) {

            var exposureMatrix = CreateExposureMatrix(
                recordsIds,
                substances,
                numberOfComponents,
                numberOfZeroExposureSubstances,
                numberOfZeroExposureRecords,
                sigma,
                seed
            );

            return new IndividualMatrix() {
                VMatrix = exposureMatrix.Exposures,
                Individuals = exposureMatrix.Individuals,
            };
        }
    }
}
