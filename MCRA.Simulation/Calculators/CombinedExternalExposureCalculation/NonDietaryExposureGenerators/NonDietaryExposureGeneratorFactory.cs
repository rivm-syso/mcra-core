using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {
    public class NonDietaryExposureGeneratorFactory {
        public static NonDietaryExposureGenerator Create(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets,
            PopulationAlignmentMethod populationAlignmentMethod,
            bool correlatedNonDietaryExposureSets,
            bool alignOnAge,
            bool alignOnSex
        ) {
            NonDietaryExposureGenerator result = populationAlignmentMethod switch {
                PopulationAlignmentMethod.MatchIndividualID => new NonDietaryMatchedExposureGenerator(nonDietaryExposureSets),
                PopulationAlignmentMethod.MatchCofactors => correlatedNonDietaryExposureSets
                    ? new NonDietaryUnmatchedExposureGenerator(nonDietaryExposureSets)
                    : new NonDietaryUnmatchedCorrelatedExposureGenerator(
                            nonDietaryExposureSets,
                            alignOnAge,
                            alignOnSex
                        ),
                PopulationAlignmentMethod.MatchRandom => throw new NotImplementedException("Match at random not implemented for non-dietary exposures"),
                _ => throw new NotImplementedException()
            };
            return result;
        }
    }
}
