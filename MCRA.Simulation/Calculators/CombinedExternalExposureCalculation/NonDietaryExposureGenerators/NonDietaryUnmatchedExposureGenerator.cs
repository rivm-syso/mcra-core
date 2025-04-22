using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

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
        protected override List<NonDietaryIntakePerCompound> generateIntakesPerSubstance(
            SimulatedIndividual individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple targetUnit,
            IRandom generator
        ) {
            var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (checkIndividualMatchesNonDietarySurvey(individual, nonDietarySurvey)
                    && nonDietarySurvey.ProportionZeros < 100
                ) {
                    if (_individualsPerSurvey.TryGetValue(nonDietarySurvey, out var individualSet)
                        && individualSet.Any()
                    ) {
                        if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                            var ix = generator.Next(0, individualSet.Count);
                            if (exposureSets.TryGetValue(individualSet.ElementAt(ix), out var exposureSet)) {
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
                    }
                }
            }
            return nonDietaryExposures;
        }
    }
}
