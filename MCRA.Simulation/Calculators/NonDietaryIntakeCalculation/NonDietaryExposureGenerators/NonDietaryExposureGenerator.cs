using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {
    public  abstract class NonDietaryExposureGenerator {

        protected TargetUnit _targetUnit;
        protected BodyWeightUnit _targetBodyWeightUnit;
        protected Dictionary<NonDietarySurvey, Dictionary<string, NonDietaryExposureSet>> _nonDietaryExposureSetsDictionary;

        public virtual void Initialize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets,
            TargetUnit targetUnit,
            BodyWeightUnit targetBodyWeightUnit
        ) {
            _targetUnit = targetUnit;
            _targetBodyWeightUnit = targetBodyWeightUnit;
            _nonDietaryExposureSetsDictionary = nonDietaryExposureSets
                .ToDictionary(r => r.Key, r => r.Value.ToDictionary(nde => nde.IndividualCode, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Generates acute non-dietary individual day exposures.
        /// </summary>
        /// <param name="individualDays"></param>
        /// <param name="substances"></param>
        /// <param name="nonDietarySurveys"></param>
        /// <param name="seed"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public List<NonDietaryIndividualDayIntake> CalculateAcuteNonDietaryIntakes(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            int seed,
            double relativeCompartmentWeight,
            CancellationToken cancelToken
        ) {
            var nonDietaryIndividualDayIntakes = individualDays
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var nonDietaryIndividualDayIntake = calculateNonDietaryIndividualDayIntake(
                        individualDay,
                        substances,
                        nonDietarySurveys,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId), true)
                    );
                    nonDietaryIndividualDayIntake.RelativeCompartmentWeight = relativeCompartmentWeight;
                    return nonDietaryIndividualDayIntake;
                })
                .OrderBy(r => r.SimulatedIndividualDayId)
                .ToList();

            // Check if success
            if (!nonDietaryIndividualDayIntakes.Any(r => r.NonDietaryIntake?.NonDietaryIntakesPerCompound?.Any() ?? false)) {
                throw new Exception("Failed to match any non-dietary exposure to a dietary exposure");
            }

            return nonDietaryIndividualDayIntakes;
        }

        /// <summary>
        /// Calculate chronic daily intakes
        /// </summary>
        /// <param name="individualDays"></param>
        /// <param name="substances"></param>
        /// <param name="nonDietarySurveys"></param>
        /// <param name="seed"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public List<NonDietaryIndividualDayIntake> CalculateChronicNonDietaryIntakes(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            int seed,
            double relativeCompartmentWeight,
            CancellationToken cancelToken
        ) {
            // Generate one non-dietary individual exposure per simulated individual
            var nonDietaryIntakeDictionary = individualDays
                .GroupBy(r => r.SimulatedIndividualId)
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .ToDictionary(r => r.Key, r => {
                    var individual = r.First().Individual;
                    var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
                    var individualRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(seed, individual.Code), true);
                    foreach (var nonDietarySurvey in nonDietarySurveys) {
                        nonDietaryExposures.AddRange(createNonDietaryIndividualExposure(
                            individual,
                            nonDietarySurvey,
                            substances,
                            individualRandomGenerator)
                        );
                    }
                    return nonDietaryExposures;
                });

            // Generate non-dietary individual day exposures from individual days and non-dietary individual exposures.
            var nonDietaryIndividualDayIntakes = individualDays
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay =>
                    new NonDietaryIndividualDayIntake() {
                        SimulatedIndividualId = individualDay.SimulatedIndividualId,
                        SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                        IndividualSamplingWeight = individualDay.IndividualSamplingWeight,
                        Day = individualDay.Day,
                        Individual = individualDay.Individual,
                        NonDietaryIntake = new NonDietaryIntake() {
                            NonDietaryIntakesPerCompound = nonDietaryIntakeDictionary[individualDay.SimulatedIndividualId]
                        },
                        RelativeCompartmentWeight = relativeCompartmentWeight,
                    }
                )
                .OrderBy(c => c.SimulatedIndividualDayId)
                .ToList();

            // Check if success
            if (!nonDietaryIndividualDayIntakes.Any(r => r.NonDietaryIntake?.NonDietaryIntakesPerCompound?.Any() ?? false)) {
                throw new Exception("Failed to match any non-dietary exposure to a dietary exposure");
            }

            return nonDietaryIndividualDayIntakes;
        }

        protected abstract List<NonDietaryIntakePerCompound> createNonDietaryIndividualExposure(
            Individual individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        /// <summary>
        /// Evaluates whether the individual matches with the non-dietary survey properties.
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="survey"></param>
        /// <returns></returns>
        protected static bool checkIndividualMatchesNonDietarySurvey(Individual individual, NonDietarySurvey survey) {
            var individualProperties = individual.IndividualPropertyValues;
            var result = survey.NonDietarySurveyProperties.All(sp => {
                var ip = individualProperties.FirstOrDefault(ips => sp.IndividualProperty == ips.IndividualProperty);
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
        /// <param name="exposureSet"></param>
        /// <param name="nonDietarySurvey"></param>
        /// <param name="individual"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        protected List<NonDietaryIntakePerCompound> nonDietaryIntakePerCompound(
            NonDietaryExposureSet exposureSet,
            NonDietarySurvey nonDietarySurvey,
            Individual individual,
            ICollection<Compound> substances
        ) {
            var correctionFactor = nonDietarySurvey.ExposureUnit.GetExposureUnitMultiplier(_targetUnit, individual.BodyWeight);
            if (_targetUnit.ConcentrationMassUnit != ConcentrationMassUnit.PerUnit) {
                correctionFactor = correctionFactor * individual.BodyWeight;
            }
            var nonDietaryExposures = exposureSet.NonDietaryExposures
                .Where(nde => substances.Contains(nde.Compound))
                .SelectMany(nde => {
                    var oral = new NonDietaryIntakePerCompound() {
                        Route = ExposureRouteType.Oral,
                        Exposure = correctionFactor * nde.Oral,
                        Compound = nde.Compound,
                    };
                    var dermal = new NonDietaryIntakePerCompound() {
                        Route = ExposureRouteType.Dermal,
                        Exposure = correctionFactor * nde.Dermal,
                        Compound = nde.Compound,
                    };
                    var inhalation = new NonDietaryIntakePerCompound() {
                        Route = ExposureRouteType.Inhalation,
                        Exposure = correctionFactor * nde.Inhalation,
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
        /// <param name="individualDay"></param>
        /// <param name="substances"></param>
        /// <param name="nonDietarySurveys"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        private NonDietaryIndividualDayIntake calculateNonDietaryIndividualDayIntake(
            IIndividualDay individualDay,
            ICollection<Compound> substances,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            IRandom generator
        ) {
            var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
            foreach (var nonDietarySurvey in nonDietarySurveys) {
                nonDietaryExposures.AddRange(createNonDietaryIndividualExposure(
                    individualDay.Individual,
                    nonDietarySurvey,
                    substances,
                    generator
                ));
            }
            return new NonDietaryIndividualDayIntake() {
                SimulatedIndividualId = individualDay.SimulatedIndividualId,
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                IndividualSamplingWeight = individualDay.IndividualSamplingWeight,
                Day = individualDay.Day,
                Individual = individualDay.Individual,
                NonDietaryIntake = new NonDietaryIntake() {
                    NonDietaryIntakesPerCompound = nonDietaryExposures,
                },
            };
        }
    }
}
