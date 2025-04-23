using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {
    public class NonDietaryMatchedExposureGenerator : NonDietaryExposureGenerator {

        protected override List<IExternalIndividualDayExposure> generate(
            IIndividualDay individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple targetUnit,
            IRandom randomIndividual
        ) {
            var externalIndividualDayExposures = new List<IExternalIndividualDayExposure>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (exposureSets.TryGetValue(individual.SimulatedIndividual.Code, out var exposureSet)
                    || exposureSets.TryGetValue("general", out exposureSet)
                ) {
                    var externalIndividualDayExposure = createExternalIndividualDayExposure(
                        exposureSet,
                        nonDietarySurvey,
                        individual,
                        substances,
                        routes,
                        targetUnit
                    );
                    if (externalIndividualDayExposure != null) {
                        externalIndividualDayExposures.Add(externalIndividualDayExposure);
                    }
                }
            }
            return externalIndividualDayExposures;
        }
    }
}

