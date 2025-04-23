using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

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
                    generator.Reset();
                    if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                        var ix = generator.Next(0, _nonDietaryIndividualCodes.Count);
                        var randomIndividualCode = _nonDietaryIndividualCodes[ix];
                        if (exposureSets.TryGetValue(randomIndividualCode, out var exposureSet)
                            && exposureSet != null
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
                }
            }
            return externalIndividualDayExposures;
        }
    }
}
