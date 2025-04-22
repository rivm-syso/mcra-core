using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators {
    public class AirExposureGeneratorFactory {

        public static AirExposureGenerator Create(PopulationAlignmentMethod populationAlignmentMethod) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new AirMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchRandom) {
                return new AirUnmatchedExposureGenerator();
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
