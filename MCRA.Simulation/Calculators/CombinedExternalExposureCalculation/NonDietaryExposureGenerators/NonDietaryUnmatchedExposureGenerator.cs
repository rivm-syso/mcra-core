using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {

    public class NonDietaryUnmatchedExposureGenerator : NonDietaryExposureGenerator {

        protected Dictionary<NonDietarySurvey, List<string>> _individualsPerSurvey = [];

        public override void Initialize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets) {
            base.Initialize(nonDietaryExposureSets);
            _individualsPerSurvey = nonDietaryExposureSets
                .ToDictionary(r => r.Key, r => r.Value.Select(e => e.IndividualCode).ToList());
        }

        /// <summary>
        /// No correlation between individuals in different  nondietary surveys
        ///  Randomly pair non-dietary and dietary individuals, no correlation between nondietary individuals
        /// (if the properties of the individual match the covariates of the non-dietary survey)
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            IIndividualDay individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple targetUnit,
            IRandom generator
        ) {
            var externalIndividualDayExposures = new List<IExternalIndividualDayExposure>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (checkIndividualMatchesNonDietarySurvey(individual.SimulatedIndividual, nonDietarySurvey)
                    && nonDietarySurvey.ProportionZeros < 100
                ) {
                    if (_individualsPerSurvey.TryGetValue(nonDietarySurvey, out var individualSet)
                        && individualSet.Any()
                    ) {
                        if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                            var ix = generator.Next(0, individualSet.Count);
                            if (exposureSets.TryGetValue(individualSet.ElementAt(ix), out var exposureSet)) {
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
                    }
                }
            }
            return externalIndividualDayExposures;
        }
    }
}
