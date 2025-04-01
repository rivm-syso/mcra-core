using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.MixtureCalculation.ExposureMatrixCalculation;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake components.
    /// </summary>
    public static class FakeExposureMatrixGenerator {

        /// <summary>
        /// Creates an exposure matrix based on components
        /// </summary>
        /// <param name="recordsIds"></param>
        /// <param name="substanceTargetCombinations"></param>
        /// <param name="numberOfComponents"></param>
        /// <param name="numberOfZeroExposureSubstances"></param>
        /// <param name="numberOfZeroExposureRecords"></param>
        /// <param name="sigma"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ExposureMatrix CreateExposureMatrix(
            List<int> recordsIds,
            List<(Compound Substance, ExposureTarget Target)> substanceTargetCombinations,
            int numberOfComponents = 4,
            int numberOfZeroExposureSubstances = 0,
            int numberOfZeroExposureRecords = 0,
            double sigma = 1D,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);

            if (numberOfZeroExposureSubstances > substanceTargetCombinations.Count) {
                throw new Exception("Number of zero exposure substances should be smaller than the number of substances.");
            }
            if (numberOfComponents > substanceTargetCombinations.Count - numberOfZeroExposureSubstances) {
                throw new Exception("Number of components should be smaller than the number of substances.");
            }

            var components = new double[numberOfComponents, substanceTargetCombinations.Count];
            for (int i = 0; i < numberOfComponents; i++) {
                for (int j = 0; j < substanceTargetCombinations.Count - numberOfZeroExposureSubstances; j++) {
                    var logNormal = new LogNormalDistribution(0, 1);
                    components[i, j] = j % numberOfComponents == i ? logNormal.Draw(random) : 0D;
                }
                for (int j = substanceTargetCombinations.Count - numberOfZeroExposureSubstances; j < substanceTargetCombinations.Count; j++) {
                    components[i, j] = 0D;
                }
            }

            var exposures = new double[substanceTargetCombinations.Count, recordsIds.Count];
            for (int i = 0; i < recordsIds.Count - numberOfZeroExposureRecords; i++) {
                var idComponent = i % numberOfComponents;
                for (int j = 0; j < substanceTargetCombinations.Count; j++) {
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
                for (int j = 0; j < substanceTargetCombinations.Count; j++) {
                    exposures[j, i] = 0D;
                }
            }

            var rowRecords = substanceTargetCombinations
                .Select((x, ix) => (ix, rowRecord: new ExposureMatrixRowRecord() {
                    Substance = substanceTargetCombinations[ix].Substance,
                    TargetUnit = new TargetUnit() { Target = substanceTargetCombinations[ix].Target },
                    Stdev = 1d
                }))
                .ToDictionary(c => c.ix, c => c.rowRecord);

            return new ExposureMatrix() {
                Exposures = new GeneralMatrix(exposures),
                SimulatedIndividuals = recordsIds.Select(c => new SimulatedIndividual(new(c) { Code = c.ToString() }, c)).ToList(),
                RowRecords = rowRecords
            };
        }

        /// <summary>
        /// Creates IndividualExposureMatrix
        /// </summary>
        /// <param name="recordsIds"></param>
        /// <param name="substanceTargetCombinations"></param>
        /// <param name="numberOfComponents"></param>
        /// <param name="numberOfZeroExposureSubstances"></param>
        /// <param name="numberOfZeroExposureRecords"></param>
        /// <param name="sigma"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IndividualMatrix CreateIndividualMatrix(
            List<int> recordsIds,
            List<(Compound Substance, ExposureTarget Target)> substanceTargetCombinations,
            int numberOfComponents = 4,
            int numberOfZeroExposureSubstances = 0,
            int numberOfZeroExposureRecords = 0,
            double sigma = 1D,
            int seed = 1
        ) {
            var exposureMatrix = CreateExposureMatrix(
                recordsIds,
                substanceTargetCombinations,
                numberOfComponents,
                numberOfZeroExposureSubstances,
                numberOfZeroExposureRecords,
                sigma,
                seed
            );
            return new IndividualMatrix() {
                VMatrix = exposureMatrix.Exposures,
                SimulatedIndividuals = exposureMatrix.SimulatedIndividuals,
            };
        }
    }
}
