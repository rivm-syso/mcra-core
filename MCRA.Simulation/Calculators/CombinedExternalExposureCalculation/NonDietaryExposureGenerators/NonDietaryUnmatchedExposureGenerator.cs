using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {

    public class NonDietaryUnmatchedExposureGenerator : NonDietaryExposureGenerator {

        private readonly Dictionary<NonDietarySurvey, List<string>> _individualsPerSurvey = [];

        public NonDietaryUnmatchedExposureGenerator(IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets) {
            _individualsPerSurvey = nonDietaryExposureSets?
                .ToDictionary(r => r.Key, r => r.Value.Select(e => e.IndividualCode).ToList());
            _nonDietaryExposureSetsDictionary = nonDietaryExposureSets?
                .ToDictionary(r => r.Key, r => r.Value.ToDictionary(nde => nde.IndividualCode, StringComparer.OrdinalIgnoreCase));

        }

        /// <summary>
        /// No correlation between individuals in different  nondietary surveys
        ///  Randomly pair non-dietary and dietary individuals, no correlation between nondietary individuals
        /// (if the properties of the individual match the covariates of the non-dietary survey)
        /// </summary>
        protected override IExternalIndividualDayExposure generate(
            IIndividualDay individualDay,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom generator
        ) {
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (checkIndividualMatchesNonDietarySurvey(individualDay.SimulatedIndividual, nonDietarySurvey)
                    && nonDietarySurvey.ProportionZeros < 100) {
                    generator.Reset();
                    if (_individualsPerSurvey.TryGetValue(nonDietarySurvey, out var individualSet)
                        && individualSet.Any()
                    ) {
                        if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                            var ix = generator.Next(0, individualSet.Count);
                            if (exposureSets.TryGetValue(individualSet.ElementAt(ix), out var exposureSet)) {
                                var externalIndividualDayExposure = createExternalIndividualDayExposure(
                                    exposureSet,
                                    individualDay,
                                    substances,
                                    routes
                                );
                                return externalIndividualDayExposure;
                            }
                        }
                    }
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
            IRandom generator
        ) {
            var externalIndividualDayExposures = new List<IExternalIndividualDayExposure>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                foreach (var individualDay in individualDays) {
                    if (checkIndividualMatchesNonDietarySurvey(simulatedIndividual, nonDietarySurvey)
                        && nonDietarySurvey.ProportionZeros < 100) {
                        generator.Reset();
                        if (_individualsPerSurvey.TryGetValue(nonDietarySurvey, out var individualSet)
                            && individualSet.Any()
                        ) {
                            if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                                var ix = generator.Next(0, individualSet.Count);
                                if (exposureSets.TryGetValue(individualSet.ElementAt(ix), out var exposureSet)) {
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
                    }
                }
            }
            return externalIndividualDayExposures;
        }
    }
}
