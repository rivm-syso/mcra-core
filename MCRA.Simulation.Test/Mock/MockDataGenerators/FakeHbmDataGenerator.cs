using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock human monitoring data
    /// </summary>
    public static class FakeHbmDataGenerator {

        /// <summary>
        /// Creates a fake HBM sample substance collection.
        /// </summary>
        public static List<HumanMonitoringSampleSubstanceCollection> FakeHbmSampleSubstanceCollections(
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            string concentrationUnitString = "mg/L",
            double? lipidGravity = null
        ) {

            var survey = FakeHbmDataGenerator.MockHumanMonitoringSurvey(individualDays);
            var hbmSamples = MockHumanMonitoringSamples(individualDays, substances, samplingMethod, concentrationUnitString, lipidGravity);
            var result = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(
                substances, 
                hbmSamples, 
                ConcentrationUnit.mgPerL,
                survey);
            return result;
        }

        /// <summary>
        /// Creates a dictionary of human monitoring samples for each survey
        /// </summary>
        /// <param name="individualDays"></param>
        /// <returns></returns>
        public static HumanMonitoringSurvey MockHumanMonitoringSurvey(
            ICollection<SimulatedIndividualDay> individualDays = null
        ) {
            var result = new HumanMonitoringSurvey() {
                Code = "HumanMonitoringSurvey",
                Description = "Description",
                Location = "Location",
                Individuals = individualDays?.Select(c => c.Individual).Distinct().ToList(),
                NumberOfSurveyDays = individualDays?.FirstOrDefault()?.Individual.NumberOfDaysInSurvey ?? 2,
            };
            return result;
        }

        /// <summary>
        /// Creates a list of human monitoring samples
        /// </summary>
        public static List<HumanMonitoringSample> MockHumanMonitoringSamples(
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            string concentrationUnitString = "mg/L",
            double? lipidGravity = null,
            int seed = 1
        ) {
            var code = string.Empty;
            var result = generateSurveyHumanMonitoringSamples(
                individualDays, 
                substances,
                samplingMethod,
                concentrationUnitString,
                lipidGravity,
                seed
            );
            return result;
        }

        /// <summary>
        /// Generates human monitoring individual day concentrations.
        /// </summary>
        /// <param name="individualDays"></param>
        /// <param name="substances"></param>
        /// <param name="samplingMethod"></param>
        /// <param name="fractionZero"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ICollection<HbmIndividualDayConcentration> MockHumanMonitoringIndividualDayConcentrations(
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            double fractionZero = .5,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = new List<HbmIndividualDayConcentration>();
            foreach (var individualDay in individualDays) {
                var individualExposure = new HbmIndividualDayConcentration() {
                    Individual = individualDay.Individual,
                    Day = individualDay.Day,
                    IndividualSamplingWeight = individualDay.IndividualSamplingWeight,
                    SimulatedIndividualId = individualDay.SimulatedIndividualId,
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    ConcentrationsBySubstance = substances
                        .ToDictionary(
                            r => r,
                            r => new HbmSubstanceTargetExposure() {
                                Substance = r,
                                Concentration = random.NextDouble() > fractionZero ? LogNormalDistribution.Draw(random, 0, 1) : 0,
                                SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                            } as IHbmSubstanceTargetExposure
                        ),
                };
                result.Add(individualExposure);
            }
            return result;
        }

        /// <summary>
        /// Creates a fake monitoring sampling method.
        /// </summary>
        /// <param name="biologicalMatrix"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="sampleType"></param>
        /// <returns></returns>
        public static HumanMonitoringSamplingMethod FakeHumanMonitoringSamplingMethod(
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.Blood,
            string sampleType = "Pooled",
            string exposureRoute = null
        ) {
            return new HumanMonitoringSamplingMethod() {
                BiologicalMatrix = biologicalMatrix,
                ExposureRoute = exposureRoute,
                SampleTypeCode = sampleType
            };
        }

        /// <summary>
        /// Generates human monitoring individual day concentrations.
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="compounds"></param>
        /// <param name="fractionZero"></param>
        /// <param name="biologicalMatrix"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="sampleType"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ICollection<HbmIndividualConcentration> MockHumanMonitoringIndividualConcentrations(
            ICollection<Individual> individuals,
            ICollection<Compound> compounds,
            double fractionZero = .5,
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.Blood,
            string exposureRoute = "Oral",
            string sampleType = "Pooled",
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = new List<HbmIndividualConcentration>();
            var samplingMethod = new HumanMonitoringSamplingMethod() {
                BiologicalMatrix = biologicalMatrix,
                ExposureRoute = exposureRoute,
                SampleTypeCode = sampleType
            };
            var idCounter = 0;
            foreach (var individual in individuals) {
                var individualExposure = new HbmIndividualConcentration() {
                    Individual = individual,
                    SimulatedIndividualId = idCounter++,
                    IndividualSamplingWeight = individual.SamplingWeight,
                    ConcentrationsBySubstance = compounds
                        .ToDictionary(
                            r => r,
                            r => new HbmSubstanceTargetExposure() {
                                Substance = r,
                                Concentration = random.NextDouble() > fractionZero ? LogNormalDistribution.Draw(random, 0, 1) : 0,
                                SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                            } as IHbmSubstanceTargetExposure
                        ),
                };
                result.Add(individualExposure);
            }
            return result;
        }

        /// <summary>
        /// Creates a human monitoring sample
        /// </summary>
        private static List<HumanMonitoringSample> generateSurveyHumanMonitoringSamples(
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            string concentrationUnitString,
            double? lipidGravity = null,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);

            return individualDays
                .Select((r, ix) => {
                    var sampleAnalyses = new List<SampleAnalysis>();
                    var analyticalMethod = new AnalyticalMethod() {
                        Code = $"{ix}_AM",
                        Description = "Description",
                        AnalyticalMethodCompounds = substances.ToDictionary(c => c, c => new AnalyticalMethodCompound() {
                            Compound = c,
                            LOD = 0.05,
                            LOQ = 0.05,
                            ConcentrationUnitString = concentrationUnitString,
                        })
                    };
                    var sample = new SampleAnalysis() {
                        Code = $"S{ix}",
                        AnalyticalMethod = analyticalMethod,
                        Concentrations = new Dictionary<Compound, ConcentrationPerSample>()
                    };
                    var concentrations = substances.ToDictionary(c => c, c => new ConcentrationPerSample() {
                        Concentration = (double?)random.NextDouble() * 100,
                        Compound = c,
                        ResTypeString = ResType.VAL.ToString(),
                        Sample = sample
                    });

                    sampleAnalyses.Add(new SampleAnalysis() {
                        Code = $"humanMonitoringSampleAnalysis_{ix}",
                        AnalyticalMethod = analyticalMethod,
                        AnalysisDate = new DateTime(),
                        Concentrations = concentrations
                    });

                    return new HumanMonitoringSample() {
                        Code = $"{ix}",
                        Individual = r.Individual,
                        SamplingMethod = samplingMethod,
                        DayOfSurvey = r.Day,
                        SpecificGravity = random.NextDouble() * .3,
                        SpecificGravityCorrectionFactor = random.NextDouble() * .1,
                        DateSampling = new DateTime(),
                        TimeOfSampling = "time",
                        SampleAnalyses = sampleAnalyses,
                        LipidGrav = lipidGravity ?? random.NextDouble() * 100,
                        LipidEnz = random.NextDouble() * 100,
                        Triglycerides = random.NextDouble() * 100,  // Normal levels of triglycerides are 0 - 150 mg/dL
                        Cholesterol = random.NextDouble() * 100,    // Normal levels of cholesterol are 125 - 200 mg/dL
                        Creatinine = random.NextDouble(20, 320),    // Normal range of creatinine in urine is 20 - 320 mg/dL
                        OsmoticConcentration = 0 //Currently unknown
                    };
                })
                .ToList();
        }
    }
}
