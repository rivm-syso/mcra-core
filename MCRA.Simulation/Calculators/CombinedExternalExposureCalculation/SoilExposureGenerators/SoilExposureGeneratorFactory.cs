using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {
    public class SoilExposureGeneratorFactory {

        public static SoilExposureGenerator Create(PopulationAlignmentMethod populationAlignmentMethod,
            bool alignOnAge,
            bool alignOnSex,
            AgeAlignmentMethod ageAlignmentMethod,
            List<double> ageBins
        ) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new SoilMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchCofactors) {
                return new SoilUnmatchedCorrelatedExposureGenerator(
                    alignOnAge,
                    alignOnSex,
                    ageAlignmentMethod,
                    ageBins
                );
            } else {
                // Match completely at random
                return new SoilUnmatchedExposureGenerator();
            }
        }
    }
}
