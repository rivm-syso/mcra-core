using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;
using MCRA.Utils.TestReporting;

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
            ConcentrationUnit concentrationUnit = ConcentrationUnit.ugPerL,
            double? lipidGravity = null
        ) {
            var survey = FakeHbmSurvey(individualDays);
            var hbmSamples = FakeHbmSamples(individualDays, substances, samplingMethod, concentrationUnit, lipidGravity);
            var result = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(
                substances,
                hbmSamples,
                survey,
                new()
            );
            result.ForEach(r => r.ConcentrationUnit = concentrationUnit);
            return result;
        }

        /// <summary>
        /// Creates multiple HBM sample substance collections.
        /// </summary>
        public static List<HumanMonitoringSampleSubstanceCollection> FakeHbmSampleSubstanceCollections(
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<HumanMonitoringSamplingMethod> samplingMethods,
            ConcentrationUnit concentrationUnit = ConcentrationUnit.ugPerL,
            double? lipidGravity = null
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var samplingMethod in samplingMethods) {
                result.AddRange(FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, concentrationUnit, lipidGravity));
            }
            return result;
        }

        /// <summary>
        /// Creates a dictionary of human monitoring samples for each survey
        /// </summary>
        public static HumanMonitoringSurvey FakeHbmSurvey(
            ICollection<SimulatedIndividualDay> individualDays = null
        ) {
            var numberOfSurveyDays = individualDays?.FirstOrDefault()?.Individual.NumberOfDaysInSurvey ?? 2;
            var result = new HumanMonitoringSurvey() {
                Code = "HumanMonitoringSurvey",
                Description = "Description",
                Location = "Location",
                Individuals = individualDays?.Select(c => c.Individual).Distinct().ToList(),
                NumberOfSurveyDays = numberOfSurveyDays,
                Timepoints = Enumerable
                    .Range(0, numberOfSurveyDays)
                    .Select(i => new HumanMonitoringTimepoint { Code = $"{i}", Name = $"Timepoint {i}", Description = $"Description of timepoint {i}" })
                    .ToList(),
            };
            return result;
        }

        /// <summary>
        /// Creates a list of human monitoring samples
        /// </summary>
        public static List<HumanMonitoringSample> FakeHbmSamples(
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            ConcentrationUnit concentrationUnit = ConcentrationUnit.ugPerL,
            double? lipidGravity = null,
            int seed = 1,
            int sampleCounterOffset = 0,
            List<(SimulatedIndividualDay, Compound)> notAnalysedSampleCompounds = null
        ) {
            var result = generateSurveyHumanMonitoringSamples(
                individualDays,
                substances,
                samplingMethod,
                concentrationUnit,
                lipidGravity,
                seed,
                sampleCounterOffset,
                notAnalysedSampleCompounds
            );
            return result;
        }

        /// <summary>
        /// Generates human monitoring individual day concentrations.
        /// </summary>
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
                    SimulatedIndividualBodyWeight = individualDay.IndividualBodyWeight,
                    ConcentrationsBySubstance = substances
                        .ToDictionary(
                            r => r,
                            r => new HbmSubstanceTargetExposure() {
                                Substance = r,
                                Exposure = random.NextDouble() > fractionZero ? LogNormalDistribution.Draw(random, 0, 1) : 0,
                                SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                            }
                        ),
                };
                result.Add(individualExposure);
            }
            return result;
        }

        /// <summary>
        /// Creates a fake monitoring sampling method.
        /// </summary>
        public static HumanMonitoringSamplingMethod FakeHumanMonitoringSamplingMethod(
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.Blood,
            string sampleType = "Spot",
            string exposureRoute = null
        ) {
            return new HumanMonitoringSamplingMethod() {
                BiologicalMatrix = biologicalMatrix,
                ExposureRoute = exposureRoute,
                SampleTypeCode = sampleType
            };
        }

        /// <summary>
        /// Creates a fake kinetic conversion factor.
        /// </summary>
        public static KineticConversionFactor FakeKineticConversionFactor(
            BiologicalMatrix biologicalMatrixFrom,
            BiologicalMatrix biologicalMatrixTo,
            Compound substance
        ) {
            return new KineticConversionFactor() {
                BiologicalMatrixFrom = biologicalMatrixFrom,
                SubstanceFrom = substance,
                BiologicalMatrixTo = biologicalMatrixTo,
                SubstanceTo = substance,
                ConversionFactor = TestUtils.GetRandomDouble(0.1, 5),
                Distribution = KineticConversionFactorDistributionType.Unspecified
            };
        }

        /// <summary>
        /// Creates a fake kinetic conversion factor model.
        /// </summary>
        public static IKineticConversionFactorModel FakeKineticConversionFactorModel(
            BiologicalMatrix biologicalMatrixFrom,
            BiologicalMatrix biologicalMatrixTo,
            Compound substance,
            DoseUnit doseUnitFrom = DoseUnit.ugPerL,
            DoseUnit doseUnitTo = DoseUnit.ugPerL
        ) {
            var kineticConversionFactor = FakeKineticConversionFactor(
                biologicalMatrixFrom,
                biologicalMatrixTo,
                substance
            );
            kineticConversionFactor.DoseUnitFrom = ExposureUnitTriple.FromDoseUnit(doseUnitFrom);
            kineticConversionFactor.DoseUnitTo = ExposureUnitTriple.FromDoseUnit(doseUnitTo);
            return KineticConversionFactorCalculatorFactory.Create(
                conversionFactor: kineticConversionFactor,
                useSubgroups: false
            );
        }

        /// <summary>
        /// Generates human monitoring individual day concentrations.
        /// </summary>
        public static ICollection<HbmIndividualConcentration> MockHumanMonitoringIndividualConcentrations(
            ICollection<Individual> individuals,
            ICollection<Compound> compounds,
            double fractionZero = .5,
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.Blood,
            string exposureRoute = "Oral",
            string sampleType = "Spot",
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
                                Exposure = random.NextDouble() > fractionZero ? LogNormalDistribution.Draw(random, 0, 1) : 0,
                                SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                            }
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
            ConcentrationUnit concentrationUnit,
            double? lipidGravity = null,
            int seed = 1,
            int sampleCounterOffset = 0,
            List<(SimulatedIndividualDay, Compound)> notAnalysedSampleCompounds = null
        ) {
            var localSeed = RandomUtils.CreateSeed(seed, samplingMethod.GetHashCode());
            var random = new McraRandomGenerator(localSeed);

            return individualDays
                .Select((r, ix) => {
                    int sampleCounter = ix + sampleCounterOffset;
                    var sampleAnalyses = new List<SampleAnalysis>();
                    var analyticalMethod = new AnalyticalMethod() {
                        Code = $"{sampleCounter}_AM",
                        Description = "Description",
                        AnalyticalMethodCompounds = substances
                            .Where(s => !notAnalysedSampleCompounds?.Contains((r, s)) ?? true)
                            .ToDictionary(c => c, c => new AnalyticalMethodCompound() {
                                Compound = c,
                                LOD = 0.05,
                                LOQ = 0.05,
                                ConcentrationUnit = concentrationUnit,
                            })
                    };
                    var sample = new SampleAnalysis() {
                        Code = $"S{sampleCounter}",
                        AnalyticalMethod = analyticalMethod,
                        Concentrations = new Dictionary<Compound, ConcentrationPerSample>()
                    };
                    var concentrations = substances
                        .Where(s => !notAnalysedSampleCompounds?.Contains((r, s)) ?? true)
                        .ToDictionary(c => c, c => new ConcentrationPerSample() {
                            Concentration = (double?)random.NextDouble() * 100,
                            Compound = c,
                            ResType = ResType.VAL,
                            Sample = sample
                        });

                    sampleAnalyses.Add(new SampleAnalysis() {
                        Code = $"humanMonitoringSampleAnalysis_{sampleCounter}",
                        AnalyticalMethod = analyticalMethod,
                        AnalysisDate = new DateTime(),
                        Concentrations = concentrations
                    });
                    return new HumanMonitoringSample() {
                        Code = $"{sampleCounter}",
                        Individual = r.Individual,
                        SamplingMethod = samplingMethod,
                        DayOfSurvey = r.Day,
                        SpecificGravity = random.NextDouble() * .3 + 1,
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
