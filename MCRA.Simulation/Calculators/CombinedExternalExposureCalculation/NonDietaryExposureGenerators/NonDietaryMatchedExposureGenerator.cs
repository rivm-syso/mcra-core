using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {
    public class NonDietaryMatchedExposureGenerator : NonDietaryExposureGenerator {

        public NonDietaryMatchedExposureGenerator(IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets) {
            _nonDietaryExposureSetsDictionary = nonDietaryExposureSets?
                .ToDictionary(r => r.Key, r => r.Value.ToDictionary(nde => nde.IndividualCode, StringComparer.OrdinalIgnoreCase));
        }

        protected override IExternalIndividualDayExposure generate(
            IIndividualDay individualDay,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom randomIndividual
        ) {
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (exposureSets.TryGetValue(individualDay.SimulatedIndividual.Code, out _)
                    || exposureSets.TryGetValue("general", out _)) {
                    var externalIndividualDayExposure = getExposure(
                        substances,
                        routes,
                        exposureSets,
                        individualDay
                    );
                    return externalIndividualDayExposure;
                }
            }
            return null;
        }

        protected override List<IExternalIndividualDayExposure> generate(
            SimulatedIndividual simulatedIndividual,
            ICollection<IIndividualDay> individualDays,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom randomIndividual
        ) {
            var externalIndividualDayExposures = new List<IExternalIndividualDayExposure>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                foreach (var individualDay in individualDays) {
                    var externalIndividualDayExposure = getExposure(
                        substances,
                        routes,
                        exposureSets,
                        individualDay
                    );
                    if (externalIndividualDayExposure != null) {
                        externalIndividualDayExposures.Add(externalIndividualDayExposure);
                    }
                }
            }
            return externalIndividualDayExposures;
        }

        private static IExternalIndividualDayExposure getExposure(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            Dictionary<string, NonDietaryExposureSet> exposureSets,
            IIndividualDay individualDay
        ) {
            if (exposureSets.TryGetValue(individualDay.SimulatedIndividual.Code, out var exposureSet)
                || exposureSets.TryGetValue("general", out exposureSet)
            ) {
                var externalIndividualDayExposure = createExternalIndividualDayExposure(
                    exposureSet,
                    individualDay,
                    substances,
                    routes
                );
                return externalIndividualDayExposure;
            }
            return null;
        }
    }
}

