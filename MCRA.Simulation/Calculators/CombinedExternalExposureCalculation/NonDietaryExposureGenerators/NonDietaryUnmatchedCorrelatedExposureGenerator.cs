using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {

    public class NonDietaryUnmatchedCorrelatedExposureGenerator : NonDietaryExposureGenerator {

        protected List<string> _nonDietaryIndividualCodes = [];
        private bool _matchOnAge { get; set; }
        private bool _matchOnSex { get; set; }

        public NonDietaryUnmatchedCorrelatedExposureGenerator(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets,
            bool alignOnAge,
            bool alignOnSex
        ) : base() {
            _nonDietaryExposureSetsDictionary = nonDietaryExposureSets?
                .ToDictionary(r => r.Key, r => r.Value.ToDictionary(nde => nde.IndividualCode, StringComparer.OrdinalIgnoreCase));

            _nonDietaryIndividualCodes = nonDietaryExposureSets?
                .SelectMany(ndeuis => ndeuis.Value.Select(r => r.IndividualCode))
                .Distinct()
                .ToList();
            _matchOnAge = alignOnAge;
            _matchOnSex = alignOnSex;
        }

        /// <summary>
        /// Use the correlation between individuals in different nondietary surveys.
        /// Randomly pair non-dietary and dietary individuals (if the properties of 
        /// the individual match the covariates of the non-dietary survey).
        /// </summary>
        protected override IExternalIndividualDayExposure generate(
            IIndividualDay individualDay,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom generator
        ) {
            var results = new List<IExternalIndividualDayExposure>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (checkIndividualMatchesNonDietarySurvey(individualDay.SimulatedIndividual, nonDietarySurvey, _matchOnSex, _matchOnAge)
                    && nonDietarySurvey.ProportionZeros < 100) {
                    generator.Reset();
                    if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                        var externalIndividualDayExposure = getExposure(individualDay,
                            substances,
                            routes,
                            generator,
                            exposureSets
                        );
                        return externalIndividualDayExposure;
                    }
                }
            }
            return null;
        }

        private IExternalIndividualDayExposure getExposure(IIndividualDay individualDay, ICollection<Compound> substances, ICollection<ExposureRoute> routes, IRandom generator, Dictionary<string, NonDietaryExposureSet> exposureSets) {
            var ix = generator.Next(0, _nonDietaryIndividualCodes.Count);
            var randomIndividualCode = _nonDietaryIndividualCodes[ix];
            if (exposureSets.TryGetValue(randomIndividualCode, out var exposureSet)
                && exposureSet != null) {
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

        /// <summary>
        /// Use the correlation between individuals in different nondietary surveys.
        /// Randomly pair non-dietary and dietary individuals
        /// (if the properties of the individual match the covariates of the non-dietary survey)
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            SimulatedIndividual simulatedIndividual,
            ICollection<IIndividualDay> individualDays,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom generator
        ) {
            var results = new List<IExternalIndividualDayExposure>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                foreach (var individualDay in individualDays) {
                    if (checkIndividualMatchesNonDietarySurvey(simulatedIndividual, nonDietarySurvey, _matchOnSex, _matchOnAge)
                        && nonDietarySurvey.ProportionZeros < 100) {
                        generator.Reset();
                        if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                            var externalIndividualDayExposure = getExposure(individualDay,
                                substances,
                                routes,
                                generator,
                                exposureSets
                            );
                            if (externalIndividualDayExposure != null) {
                                results.Add(externalIndividualDayExposure);
                            }
                        }
                    }
                }
            }
            return results;
        }
    }
}
