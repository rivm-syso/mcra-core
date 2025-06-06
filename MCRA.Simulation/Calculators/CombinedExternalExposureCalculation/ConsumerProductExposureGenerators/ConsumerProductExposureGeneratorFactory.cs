using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.ConsumerProductExposureGenerators {
    public class ConsumerProductExposureGeneratorFactory {

        public static ConsumerProductExposureGenerator Create(
            PopulationAlignmentMethod populationAlignmentMethod,
            bool alignOnAge,
            bool alignOnSex,
            AgeAlignmentMethod ageAlignmentMethod,
            List<double> ageBins
        ) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new ConsumerProductMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchCofactors) {
                return new ConsumerProductUnmatchedCorrelatedExposureGenerator(
                    alignOnAge,
                    alignOnSex,
                    ageAlignmentMethod,
                    ageBins
                );
            } else {
                // Match completely at random
                return new ConsumerProductUnmatchedExposureGenerator();
            }
        }
    }
}
