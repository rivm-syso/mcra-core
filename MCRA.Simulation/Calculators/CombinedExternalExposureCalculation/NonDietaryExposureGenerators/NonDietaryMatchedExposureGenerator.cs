using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {
    public class NonDietaryMatchedExposureGenerator : NonDietaryExposureGenerator {

        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom randomIndividual
        ) {
            var externalIndividualDayExposures = new List<IExternalIndividualDayExposure>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                foreach (var individualDay in individualDays) {
                    if (exposureSets.TryGetValue(individualDay.SimulatedIndividual.Code, out var exposureSet)
                        || exposureSets.TryGetValue("general", out exposureSet)
                    ) {
                        var externalIndividualDayExposure = createExternalIndividualDayExposure(
                            exposureSet,
                            individualDay,
                            substances,
                            routes
                        );
                        if (externalIndividualDayExposure != null) {
                            externalIndividualDayExposures.Add(externalIndividualDayExposure);
                        }
                    }
                }
            }
            return externalIndividualDayExposures;
        }
    }
}

