using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {
    public class SoilExposureGeneratorFactory {

        public static SoilExposureGenerator Create(PopulationAlignmentMethod populationAlignmentMethod) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new SoilMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchRandom) {
                return new SoilUnmatchedExposureGenerator();
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
