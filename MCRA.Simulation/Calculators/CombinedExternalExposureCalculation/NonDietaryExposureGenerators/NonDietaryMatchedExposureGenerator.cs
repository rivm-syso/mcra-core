using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {
    public class NonDietaryMatchedExposureGenerator : NonDietaryExposureGenerator {

        protected override List<NonDietaryIntakePerCompound> generateIntakesPerSubstance(
            SimulatedIndividual individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple targetUnit,
            IRandom randomIndividual
        ) {
            var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (exposureSets.TryGetValue(individual.Code, out var exposureSet)
                    || exposureSets.TryGetValue("general", out exposureSet)
                ) {
                    var individualDayExposure = nonDietaryIntakePerCompound(
                        exposureSet,
                        nonDietarySurvey,
                        individual,
                        substances,
                        routes,
                        targetUnit
                    );
                    nonDietaryExposures.AddRange(individualDayExposure);
                }
            }
            return nonDietaryExposures;
        }
    }
}

