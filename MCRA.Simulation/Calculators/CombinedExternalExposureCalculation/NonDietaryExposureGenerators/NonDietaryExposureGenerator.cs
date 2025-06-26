using CommandLine;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.IndividualExposures;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators {
    public abstract class NonDietaryExposureGenerator {

        protected Dictionary<NonDietarySurvey, Dictionary<string, NonDietaryExposureSet>> _nonDietaryExposureSetsDictionary;

        /// <summary>
        /// Generates acute non-dietary individual day exposures for the
        /// specified collection of individual days.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            ExternalExposureUnit externalExposureUnit,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureType exposureType,
            int seed
        ) {
            var result = exposureType == ExposureType.Acute
                ? GenerateAcute(
                    individualDays,
                    nonDietarySurveys,
                    externalExposureUnit,
                    substances,
                    routes,
                    seed
                )
                : GenerateChronic(
                    individualDays,
                    nonDietarySurveys,
                    externalExposureUnit,
                    substances,
                    routes,
                    seed
                );
            return result;
        }

        /// <summary>
        /// Generates acute non-dietary individual day exposures.
        /// </summary>
        public ExternalExposureCollection GenerateAcute(
            ICollection<IIndividualDay> individualDays,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            ExternalExposureUnit externalExposureUnit,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            int seed
        ) {
            var externalIndividualDayExposures = individualDays
                .AsParallel()
                .Select(individualDay => generateIndividualExposure(
                    individualDay,
                    substances,
                    routes,
                    nonDietarySurveys,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                ))
                .OrderBy(r => r.SimulatedIndividualDayId)
                .ToList();

            // Check if success
            if (externalIndividualDayExposures.Count == 0) {
                throw new Exception("Failed to match any non-dietary exposures.");
            }
            var exposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = externalExposureUnit.GetSubstanceAmountUnit(),
                ExposureSource = ExposureSource.OtherNonDiet,
                ExternalIndividualDayExposures = externalIndividualDayExposures
                    .Cast<IExternalIndividualDayExposure>()
                    .ToList()
            };
            return exposureCollection;
        }

        /// <summary>
        /// Calculate chronic daily intakes
        /// </summary>
        public ExternalExposureCollection GenerateChronic(
            ICollection<IIndividualDay> individualDays,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            ExternalExposureUnit externalExposureUnit,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            int seed
        ) {
            // Generate non-dietary individual day exposures from individual days and non-dietary individual exposures.
            var externalIndividualDayExposures = individualDays
                .GroupBy(r => r.SimulatedIndividual)
                .AsParallel()
                .SelectMany(individualExposures => generateIndividualExposure(
                    individualExposures.Key,
                    [.. individualExposures],
                    substances,
                    routes,
                    nonDietarySurveys,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key.Code))
                ))
                .OrderBy(r => r.SimulatedIndividualDayId)
                .ToList();

            // Check if success


            if (externalIndividualDayExposures.Count == 0) {
                throw new Exception("Failed to match any non-dietary exposure");
            }
            var exposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = externalExposureUnit.GetSubstanceAmountUnit(),
                ExposureSource = ExposureSource.OtherNonDiet,
                ExternalIndividualDayExposures = externalIndividualDayExposures
                    .Cast<IExternalIndividualDayExposure>()
                    .ToList()
            };
            return exposureCollection;
        }

        protected abstract List<IExternalIndividualDayExposure> generate(
            SimulatedIndividual simulatedIndividual,
            ICollection<IIndividualDay> individualDays,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom randomIndividual
        );

        protected abstract IExternalIndividualDayExposure generate(
            IIndividualDay individualDay,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom randomIndividual
        );

        /// <summary>
        /// Evaluates whether the individual matches with the non-dietary survey properties.
        /// </summary>
        protected static bool checkIndividualMatchesNonDietarySurvey(
            SimulatedIndividual individual,
            NonDietarySurvey survey,
            bool alignOnSex = false,
            bool alignOnAge = false
        ) {
            var result = survey.NonDietarySurveyProperties.All(sp => {
                var matchAge = false;
                var matchSex = false;

                var ip = individual.Individual.GetPropertyValue(sp.IndividualProperty);
                if (ip != null) {
                    //Set matchAge always to true if property is cofactor (sex)
                    if (sp.PropertyType == PropertyType.Cofactor) {
                        matchAge = true;
                        //Evaluate cofactor sex and set matchSex; when sex is not relevant (alignOnSex = false) set matchSex to true
                        if (alignOnSex) {
                            if (sp.IndividualPropertyTextValue == ip.Value) {
                                matchSex = true;
                            }
                        } else {
                            matchSex = true;
                        }
                    }

                    //Set matchSex always to true if property is covariable (age)
                    if (sp.PropertyType == PropertyType.Covariable) {
                        matchSex = true;
                        //Evaluate covariable age and set matchAge; when age is not relevant (alignOnAge = false) set matchAge to true
                        if (alignOnAge) {
                            if (sp.IndividualPropertyDoubleValueMin <= ip.DoubleValue
                                && sp.IndividualPropertyDoubleValueMax >= ip.DoubleValue) {
                                matchAge = true;
                            }
                        } else {
                            matchAge = true;
                        }
                    }

                    var match = matchSex && matchAge;
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
        protected static IExternalIndividualDayExposure createExternalIndividualDayExposure(
            NonDietaryExposureSet exposureSet,
            IIndividualDay individualDay,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes
        ) {
            var surveyExposureUnit = exposureSet.NonDietarySurvey.ExposureUnit;
            var correctionFactor = surveyExposureUnit.GetConcentrationMassUnit() != ConcentrationMassUnit.PerUnit
                ? individualDay.SimulatedIndividual.BodyWeight
                : 1;

            var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
            foreach (var nde in exposureSet.NonDietaryExposures) {
                if (substances.Contains(nde.Compound)) {
                    if (routes.Contains(ExposureRoute.Oral)) {
                        getExposurePerRoute(
                            exposuresPerPath,
                            nde.Compound,
                            ExposureRoute.Oral,
                            correctionFactor * nde.Oral
                        );
                    }
                    if (routes.Contains(ExposureRoute.Dermal)) {
                        getExposurePerRoute(
                            exposuresPerPath,
                            nde.Compound,
                            ExposureRoute.Dermal,
                            correctionFactor * nde.Dermal
                        );
                    }
                    if (routes.Contains(ExposureRoute.Inhalation)) {
                        getExposurePerRoute(
                            exposuresPerPath,
                            nde.Compound,
                            ExposureRoute.Inhalation,
                            correctionFactor * nde.Inhalation
                        );
                    }
                }
            }
            if (exposuresPerPath.Count > 0) {
                var externalExposure = new ExternalIndividualDayExposure(exposuresPerPath) {
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    SimulatedIndividual = individualDay.SimulatedIndividual,
                    Day = individualDay.Day
                };
                return externalExposure;
            } else {
                return null;
            }
        }

        private static void getExposurePerRoute(
            Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath,
            Compound substance,
            ExposureRoute route,
            double correctedAmount
        ) {
            var exposure = new ExposurePerSubstance() {
                Amount = correctedAmount,
                Compound = substance,
            };
            if (!exposuresPerPath.TryAdd(new(ExposureSource.OtherNonDiet, route), [exposure])) {
                exposuresPerPath[new(ExposureSource.OtherNonDiet, route)].Add(exposure);
            }
        }

        /// <summary>
        /// Simulates external individual days for acute
        /// </summary>
        private IExternalIndividualDayExposure generateIndividualExposure(
            IIndividualDay individualDays,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            IRandom generator
        ) {
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var nonDietarySurvey in nonDietarySurveys) {
                var externalIndividualDayExposure = generate(
                    individualDays,
                    nonDietarySurvey,
                    substances,
                    routes,
                    generator
                );
                results.Add(externalIndividualDayExposure);
            }

            //aggregate all exposures for multiple surveys, do this on a per day basis
            var grouping = results
                .GroupBy(c => c.Day)
                .ToList();

            var pathWays = results
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
            foreach (var group in grouping) {
                foreach (var pathWay in pathWays) {
                    var intakesPerSubstance = group.SelectMany(c => c.ExposuresPerPath[pathWay])
                        .GroupBy(c => c.Compound)
                        .Select(c => new AggregateIntakePerCompound() {
                            Compound = c.Key,
                            Amount = c.Sum(r => r.Amount)
                        })
                        .Cast<IIntakePerCompound>().ToList();
                    exposuresPerPath[pathWay] = intakesPerSubstance;
                }
            }
            var individualDayExposure = grouping.SelectMany(c => c).First();
            var externalExposure = new ExternalIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividualDayId = individualDayExposure.SimulatedIndividualDayId,
                SimulatedIndividual = individualDayExposure.SimulatedIndividual,
                Day = individualDayExposure.Day
            };
            return externalExposure;
        }

        /// <summary>
        /// Simulates external individual days for chronic
        /// </summary>
        private List<IExternalIndividualDayExposure> generateIndividualExposure(
            SimulatedIndividual simulatedIndividual,
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ICollection<NonDietarySurvey> nonDietarySurveys,
            IRandom generator
        ) {
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var nonDietarySurvey in nonDietarySurveys) {
                var externalIndividualDayExposure = generate(
                    simulatedIndividual,
                    individualDays,
                    nonDietarySurvey,
                    substances,
                    routes,
                    generator
                );
                results.AddRange(externalIndividualDayExposure);
            }

            //aggregate all exposures for multiple surveys, do this on a per day basis
            var externalIndividualDayExposures = new List<IExternalIndividualDayExposure>();
            var grouping = results
                .GroupBy(c => c.Day)
                .ToList();

            var pathWays = results
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            foreach (var group in grouping) {
                var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
                foreach (var pathWay in pathWays) {
                    var intakesPerSubstance = group.SelectMany(c => c.ExposuresPerPath[pathWay])
                        .GroupBy(c => c.Compound)
                        .Select(c => new AggregateIntakePerCompound() {
                            Compound = c.Key,
                            Amount = c.Sum(r => r.Amount)
                        })
                        .Cast<IIntakePerCompound>().ToList();
                    exposuresPerPath[pathWay] = intakesPerSubstance;
                }
                var individualDayExposure = group.First();
                var externalExposure = new ExternalIndividualDayExposure(exposuresPerPath) {
                    SimulatedIndividualDayId = individualDayExposure.SimulatedIndividualDayId,
                    SimulatedIndividual = individualDayExposure.SimulatedIndividual,
                    Day = individualDayExposure.Day
                };
                externalIndividualDayExposures.Add(externalExposure);
            }

            return externalIndividualDayExposures;
        }
    }
}
