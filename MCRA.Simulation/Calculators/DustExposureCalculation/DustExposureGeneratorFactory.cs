using MCRA.General;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustExposureGeneratorFactory {

        public static DustExposureGenerator Create(PopulationAlignmentMethod populationAlignmentMethod) {
            if (populationAlignmentMethod == PopulationAlignmentMethod.MatchIndividualID) {
                return new DustMatchedExposureGenerator();
            } else if (populationAlignmentMethod == PopulationAlignmentMethod.MatchRandom) {
                return new DustUnmatchedExposureGenerator();
            } else {
                throw new NotImplementedException();
                return new DustUnmatchedCorrelatedExposureGenerator();
            }
        }
    }
}
