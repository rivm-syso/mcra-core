using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DustExposureGenerators {
    public class DustExposureGeneratorFactory {

        public static DustExposureGenerator Create(
            PopulationAlignmentMethod populationAlignmentMethod,
            bool alignOnAge,
            bool alignOnSex,
            AgeAlignmentMethod ageAlignmentMethod,
            List<double> ageBins
        ) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new DustMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchCofactors) {
                return new DustUnmatchedCorrelatedExposureGenerator(
                   alignOnAge,
                   alignOnSex,
                   ageAlignmentMethod,
                   ageBins
               );
            } else {
                // Match completely at random
                return new DustUnmatchedExposureGenerator();
            }
        }
    }
}
