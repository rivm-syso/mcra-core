using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DietExposureGenerator {
    public class DietExposureGeneratorFactory {

        public static DietExposureGenerator Create(PopulationAlignmentMethod populationAlignmentMethod) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new DietMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchRandom) {
                return new DietUnmatchedExposureGenerator();
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
