using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {
    public abstract class NonDietaryExposureGenerator {

        protected ExposureUnitTriple _targetUnit;
        protected Dictionary<NonDietarySurvey, Dictionary<string, NonDietaryExposureSet>> _nonDietaryExposureSetsDictionary;

        public virtual void Initialize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets,
            ExposureUnitTriple targetUnit
        ) {
            _targetUnit = targetUnit;
            _nonDietaryExposureSetsDictionary = nonDietaryExposureSets
                .ToDictionary(r => r.Key, r => r.Value.ToDictionary(nde => nde.IndividualCode, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Generates acute non-dietary individual day exposures.
        /// </summary>
        public List<NonDietaryIndividualDayIntake> GenerateAcuteNonDietaryIntakes(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            int seed,
            CancellationToken cancelToken
        ) {
            var nonDietaryIndividualDayIntakes = individualDays
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var nonDietaryIndividualDayIntake = generateNonDietaryIndividualDayIntake(
                        individualDay,
                        substances,
                        nonDietarySurveys,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                    );
                    return nonDietaryIndividualDayIntake;
                })
                .OrderBy(r => r.SimulatedIndividualDayId)
                .ToList();

            // Check if success
            if (!nonDietaryIndividualDayIntakes.Any(r => r.NonDietaryIntake?.NonDietaryIntakesPerCompound?.Count > 0)) {
                throw new Exception("Failed to match any non-dietary exposure to a dietary exposure");
            }
            return nonDietaryIndividualDayIntakes;
        }

        /// <summary>
        /// Calculate chronic daily intakes
        /// </summary>
        public List<NonDietaryIndividualDayIntake> GenerateChronicNonDietaryIntakes(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            int seed,
            CancellationToken cancelToken
        ) {
            // Generate non-dietary individual day exposures from individual days and non-dietary individual exposures.
            var nonDietaryIndividualDayIntakes = individualDays
                .GroupBy(r => r.SimulatedIndividual.Id, (key, g) => g.First())
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var nonDietaryIndividualDayIntake = generateNonDietaryIndividualDayIntake(
                        individualDay,
                        substances,
                        nonDietarySurveys,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividual.Code))
                    );
                    return nonDietaryIndividualDayIntake;
                })
                .OrderBy(r => r.SimulatedIndividualDayId)
                .ToList();

            // Check if success
            if (!nonDietaryIndividualDayIntakes.Any(r => r.NonDietaryIntake?.NonDietaryIntakesPerCompound?.Count > 0)) {
                throw new Exception("Failed to match any non-dietary exposure to a dietary exposure");
            }

            return nonDietaryIndividualDayIntakes;
        }

        protected abstract List<NonDietaryIntakePerCompound> createNonDietaryIndividualExposure(
            SimulatedIndividual individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        /// <summary>
        /// Evaluates whether the individual matches with the non-dietary survey properties.
        /// </summary>
        protected static bool checkIndividualMatchesNonDietarySurvey(SimulatedIndividual individual, NonDietarySurvey survey) {
            var result = survey.NonDietarySurveyProperties.All(sp => {
                var ip = individual.Individual.GetPropertyValue(sp.IndividualProperty);
                if (ip != null) {
                    var match = ((sp.PropertyType == PropertyType.Cofactor) && (sp.IndividualPropertyTextValue == ip.Value))
                        || ((sp.PropertyType == PropertyType.Covariable) && (sp.IndividualPropertyDoubleValueMin <= ip.DoubleValue && sp.IndividualPropertyDoubleValueMax >= ip.DoubleValue));
                    return match;
                } else {
                    return true;
                }
            });
            return result;
        }

        /// <summary>
        /// Extract the non dietary exposures per substance from the exposures
        /// Correct non-dietary exposure units using the target intake unit.
        /// </summary>
        protected List<NonDietaryIntakePerCompound> nonDietaryIntakePerCompound(
            NonDietaryExposureSet exposureSet,
            NonDietarySurvey nonDietarySurvey,
            SimulatedIndividual individual,
            ICollection<Compound> substances
        ) {
            var correctionFactor = nonDietarySurvey.ExposureUnit
                .GetExposureUnitMultiplier(
                    _targetUnit,
                    individual.BodyWeight
                );
            if (_targetUnit.ConcentrationMassUnit != ConcentrationMassUnit.PerUnit) {
                correctionFactor = correctionFactor * individual.BodyWeight;
            }
            var nonDietaryExposures = exposureSet.NonDietaryExposures
                .Where(nde => substances.Contains(nde.Compound))
                .SelectMany(nde => {
                    var oral = new NonDietaryIntakePerCompound() {
                        Route = ExposureRoute.Oral,
                        Amount = correctionFactor * nde.Oral,
                        Compound = nde.Compound,
                    };
                    var dermal = new NonDietaryIntakePerCompound() {
                        Route = ExposureRoute.Dermal,
                        Amount = correctionFactor * nde.Dermal,
                        Compound = nde.Compound,
                    };
                    var inhalation = new NonDietaryIntakePerCompound() {
                        Route = ExposureRoute.Inhalation,
                        Amount = correctionFactor * nde.Inhalation,
                        Compound = nde.Compound,
                    };
                    return new List<NonDietaryIntakePerCompound> { oral, dermal, inhalation };
                })
                .ToList();

            return nonDietaryExposures;
        }

        /// <summary>
        /// Simulates the acute individual days.
        /// </summary>
        private NonDietaryIndividualDayIntake generateNonDietaryIndividualDayIntake(
            IIndividualDay individualDay,
            ICollection<Compound> substances,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            IRandom generator
        ) {
            var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
            foreach (var nonDietarySurvey in nonDietarySurveys) {
                nonDietaryExposures.AddRange(createNonDietaryIndividualExposure(
                    individualDay.SimulatedIndividual,
                    nonDietarySurvey,
                    substances,
                    generator
                ));
            }
            return new NonDietaryIndividualDayIntake() {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                Day = individualDay.Day,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                NonDietaryIntake = new NonDietaryIntake() {
                    NonDietaryIntakesPerCompound = nonDietaryExposures,
                },
            };
        }
    }
}
