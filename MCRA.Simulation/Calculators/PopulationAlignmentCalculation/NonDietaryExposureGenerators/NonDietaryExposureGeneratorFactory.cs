using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.NonDietaryExposureGenerators {
    public class NonDietaryExposureGeneratorFactory {
        public static NonDietaryExposureGenerator Create(
            PopulationAlignmentMethod populationAlignmentMethod,
            bool correlatedNonDietaryExposureSets
        ) {
            NonDietaryExposureGenerator result = populationAlignmentMethod switch {
                PopulationAlignmentMethod.MatchIndividualID => new NonDietaryMatchedExposureGenerator(),
                PopulationAlignmentMethod.MatchCofactors => correlatedNonDietaryExposureSets
                    ? new NonDietaryUnmatchedCorrelatedExposureGenerator()
                    : new NonDietaryUnmatchedExposureGenerator(),
                PopulationAlignmentMethod.MatchRandom => throw new NotImplementedException("Match at random not implemented"),
                _ => throw new NotImplementedException()
            };
            return result;
        }
    }
}
