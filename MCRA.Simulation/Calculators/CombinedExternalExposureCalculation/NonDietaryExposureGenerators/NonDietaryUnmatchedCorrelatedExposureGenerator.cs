using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {

    public class NonDietaryUnmatchedCorrelatedExposureGenerator : NonDietaryExposureGenerator {

        protected List<string> _nonDietaryIndividualCodes = [];

        public override void Initialize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets) {
            base.Initialize(nonDietaryExposureSets);
            _nonDietaryIndividualCodes = nonDietaryExposureSets
                .SelectMany(ndeuis => ndeuis.Value.Select(r => r.IndividualCode))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Use the correlation between individuals in different nondietary surveys.
        /// Randomly pair non-dietary and dietary individuals
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
                    generator.Reset();
                    if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                        var ix = generator.Next(0, _nonDietaryIndividualCodes.Count);
                        var randomIndividualCode = _nonDietaryIndividualCodes[ix];
                        if (exposureSets.TryGetValue(randomIndividualCode, out var exposureSet)
                            && exposureSet != null
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
                }
            }
            return nonDietaryExposures;
        }
    }
}
