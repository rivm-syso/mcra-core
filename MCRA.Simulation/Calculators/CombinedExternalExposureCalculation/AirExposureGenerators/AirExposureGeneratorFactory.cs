using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators {
    public class AirExposureGeneratorFactory {

        public static AirExposureGenerator Create(
            PopulationAlignmentMethod populationAlignmentMethod,
            bool alignOnAge,
            bool alignOnSex,
            AgeAlignmentMethod ageAlignmentMethod,
            List<double> ageBins
        ) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new AirMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchCofactors) {
                return new AirUnmatchedCorrelatedExposureGenerator(
                    alignOnAge,
                    alignOnSex,
                    ageAlignmentMethod,
                    ageBins
                );
            } else {
                // Match completely at random
                return new AirUnmatchedExposureGenerator();
            }
        }
    }
}
