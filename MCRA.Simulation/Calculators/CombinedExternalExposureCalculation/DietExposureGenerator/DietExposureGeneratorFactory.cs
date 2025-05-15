using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DietExposureGenerator {
    public class DietExposureGeneratorFactory {

        public static DietExposureGenerator Create(
            PopulationAlignmentMethod populationAlignmentMethod,
            bool alignOnAge,
            bool alignOnSex,
            AgeAlignmentMethod ageAlignmentMethod,
            List<double> ageBins
        ) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new DietMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchCofactors) {
                return new DietUnmatchedCorrelatedExposureGenerator(
                    alignOnAge,
                    alignOnSex,
                    ageAlignmentMethod,
                    ageBins
                );
            } else {
                // Match completely at random
                return new DietUnmatchedExposureGenerator();
            }
        }
    }
}
