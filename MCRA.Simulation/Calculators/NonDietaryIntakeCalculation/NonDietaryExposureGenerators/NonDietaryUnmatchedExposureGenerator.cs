using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    public class NonDietaryUnmatchedExposureGenerator : NonDietaryExposureGenerator {

        protected Dictionary<NonDietarySurvey, List<string>> _individualsPerSurvey = [];

        public override void Initialize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets,
            HashSet<ExposureRoute> routes,
            ExposureUnitTriple targetUnit
        ) {
            base.Initialize(
                nonDietaryExposureSets,
                routes,
                targetUnit
            );
            _individualsPerSurvey = nonDietaryExposureSets
                .ToDictionary(r => r.Key, r => r.Value.Select(e => e.IndividualCode).ToList());
        }

        /// <summary>
        /// No correlation between individuals in different  nondietary surveys
        ///  Randomly pair non-dietary and dietary individuals, no correlation between nondietary individuals
        /// (if the properties of the individual match the covariates of the non-dietary survey)
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="nonDietarySurvey"></param>
        /// <param name="substances"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        protected override List<NonDietaryIntakePerCompound> createNonDietaryIndividualExposure(
            SimulatedIndividual individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (checkIndividualMatchesNonDietarySurvey(individual, nonDietarySurvey) && nonDietarySurvey.ProportionZeros < 100) {
                    if (_individualsPerSurvey.TryGetValue(nonDietarySurvey, out var individualSet) && individualSet.Any()) {
                        if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                            var ix = generator.Next(0, individualSet.Count);
                            if (exposureSets.TryGetValue(individualSet.ElementAt(ix), out var exposureSet)) {
                                nonDietaryExposures.AddRange(nonDietaryIntakePerCompound(exposureSet, nonDietarySurvey, individual, substances));
                            }
                        }
                    }
                }
            }
            return nonDietaryExposures;
        }
    }
}
