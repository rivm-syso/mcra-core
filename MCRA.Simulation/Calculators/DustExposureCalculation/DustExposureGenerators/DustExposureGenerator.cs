using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public abstract class DustExposureGenerator {

        protected ExposureUnitTriple _targetUnit;
        protected BodyWeightUnit _targetBodyWeightUnit;
        protected List<DustIndividualDayExposure> _dustIndividualDayExposures;

        public virtual void Initialize(
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ExposureUnitTriple targetUnit,
            BodyWeightUnit targetBodyWeightUnit
        ) {
            _targetUnit = targetUnit;
            _targetBodyWeightUnit = targetBodyWeightUnit;
            _dustIndividualDayExposures = dustIndividualDayExposures
                .ToList();
        }

        /// <summary>
        /// Generates acute non-dietary individual day exposures.
        /// </summary>
        /// <param name="individualDays"></param>
        /// <param name="substances"></param>
        /// <param name="dustIndividualDayExposures"></param>
        /// <param name="seed"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public List<DustIndividualDayExposure> CalculateAcuteDustExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            int seed,
            CancellationToken cancelToken
        ) {
            var dustIndividualExposures = individualDays
                //.AsParallel()
                //.WithCancellation(cancelToken)
                //.WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var dustIndividualDayExposure = createDustIndividualExposure(
                        individualDay,
                        dustIndividualDayExposures,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                    );
                    //nonDietaryIndividualDayIntake.RelativeCompartmentWeight = 1;
                    return dustIndividualDayExposure;
                })
                .SelectMany(r => r)
                .ToList();

            // Check if success
            if (dustIndividualExposures.Count() == 0) {
                throw new Exception("Failed to match any dust exposure to a dietary exposure");
            }
            return dustIndividualExposures;
        }

        /// <summary>
        /// Calculate chronic daily intakes
        /// </summary>
        /// <param name="individualDays"></param>
        /// <param name="substances"></param>
        /// <param name="dustIndividualDayExposures"></param>
        /// <param name="seed"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public List<DustIndividualDayExposure> CalculateChronicDustExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            int seed,
            CancellationToken cancelToken
        ) {
            // Generate one non-dietary individual exposure per simulated individual
            var dustIndividualExposures = individualDays
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var dustIndividualDayExposure = createDustIndividualExposure(
                        individualDay,
                        dustIndividualDayExposures,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                    );
                    //nonDietaryIndividualDayIntake.RelativeCompartmentWeight = 1;
                    return dustIndividualDayExposure;
                })
                .SelectMany(r => r)
                .ToList();
            
            // Check if success
            if (dustIndividualExposures.Count() == 0) {
                throw new Exception("Failed to match any dust exposure to a dietary exposure");
            }
            return dustIndividualExposures;
        }

        protected abstract List<DustIndividualDayExposure> createDustIndividualExposure(
            IIndividualDay individual,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        /*

        /// <summary>
        /// Evaluates whether the individual matches with the non-dietary survey properties.
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="survey"></param>
        /// <returns></returns>
        protected static bool checkIndividualMatchesDustExposures(Individual individual, NonDietarySurvey survey) {
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
                        Route = ExposurePathType.Oral,
                        Amount = correctionFactor * nde.Oral,
                        Compound = nde.Compound,
                    };
                    var dermal = new NonDietaryIntakePerCompound() {
                        Route = ExposurePathType.Dermal,
                        Amount = correctionFactor * nde.Dermal,
                        Compound = nde.Compound,
                    };
                    var inhalation = new NonDietaryIntakePerCompound() {
                        Route = ExposurePathType.Inhalation,
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
        /// <param name="individualDay"></param>
        /// <param name="substances"></param>
        /// <param name="nonDietarySurveys"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        private DustIndividualDayExposure calculateDustIndividualDayExposure(
            IIndividualDay individualDay,
            ICollection<Compound> substances,
            List<DustIndividualDayExposure> dustIndividualDayExposures,
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

        */
    }
}
