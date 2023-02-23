using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {
    public class NonDietaryMatchedExposureGenerator : NonDietaryExposureGenerator {

        protected override List<NonDietaryIntakePerCompound> createNonDietaryIndividualExposure(
            Individual individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (exposureSets.TryGetValue(individual.Code, out var exposureSet) || exposureSets.TryGetValue("general", out exposureSet)) {
                    nonDietaryExposures.AddRange(nonDietaryIntakePerCompound(exposureSet, nonDietarySurvey, individual, substances));
                }
            }
            return nonDietaryExposures;
        }
    }
}

