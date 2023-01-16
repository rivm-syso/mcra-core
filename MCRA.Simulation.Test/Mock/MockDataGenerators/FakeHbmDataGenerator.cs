using System;
using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
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
            string concentrationUnitString = "mg/L"
        ) {
            var hbmSamples = MockHumanMonitoringSamples(individualDays, substances, samplingMethod, concentrationUnitString);
            var result = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(substances, hbmSamples, ConcentrationUnit.mgPerL);
            return result;
        }

        /// <summary>
        /// Creates a dictionary of human monitoring samples for each survey
        /// </summary>
        /// <param name="individualDays"></param>
        /// <returns></returns>
        public static List<HumanMonitoringSurvey> MockHumanMonitoringSurveys(
            ICollection<SimulatedIndividualDay> individualDays = null
        ) {
            var result = new List<HumanMonitoringSurvey>();
            result.Add(new HumanMonitoringSurvey() {
                Code = "HumanMonitoringSurvey",
                Description = "Description",
                Location = "Location",
                Individuals = individualDays?.Select(c => c.Individual).Distinct().ToList(),
                NumberOfSurveyDays = individualDays?.FirstOrDefault()?.Individual.NumberOfDaysInSurvey ?? 2,
            });
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
            int seed = 1
        ) {
            var code = string.Empty;
            var result = generateSurveyHumanMonitoringSamples(
                individualDays, 
                substances,
                samplingMethod,
                concentrationUnitString,
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
                    SimulatedIndividualId = individualDay.SimulatedIndividualDayId,
                    ConcentrationsBySubstance = substances
                        .ToDictionary(
                            r => r,
                            r => new HbmConcentrationByMatrixSubstance() {
                                Substance = r,
                                Concentration = random.NextDouble() > fractionZero ? LogNormalDistribution.Draw(random, 0, 1) : 0,
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
        /// <param name="biologicalMatrix"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="sampleType"></param>
        /// <returns></returns>
        public static HumanMonitoringSamplingMethod FakeHumanMonitoringSamplingMethod(
            string biologicalMatrix = "Blood",
            string exposureRoute = null,
            string sampleType = "Pooled"
        ) {
            return new HumanMonitoringSamplingMethod() {
                Compartment = biologicalMatrix,
                ExposureRoute = exposureRoute,
                SampleType = sampleType
            };
        }

        /// <summary>
        /// Generates human monitoring individual day concentrations.
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="compounds"></param>
        /// <param name="fractionZero"></param>
        /// <param name="compartment"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="sampleType"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ICollection<HbmIndividualConcentration> MockHumanMonitoringIndividualConcentrations(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> compounds,
            double fractionZero = .5,
            string compartment = "Liver",
            string exposureRoute = "Oral",
            string sampleType = "Pooled",
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = new List<HbmIndividualConcentration>();
            var samplingMethod = new HumanMonitoringSamplingMethod() {
                Compartment = compartment,
                ExposureRoute = exposureRoute,
                SampleType = sampleType
            };
            foreach (var individualDay in simulatedIndividualDays) {
                var individualExposure = new HbmIndividualConcentration() {
                    Individual = individualDay.Individual,
                    SimulatedIndividualId = individualDay.SimulatedIndividualDayId,
                    ConcentrationsBySubstance = compounds
                        .ToDictionary(
                            r => r,
                            r => new HbmConcentrationByMatrixSubstance() {
                                Substance = r,
                                Concentration = random.NextDouble() > fractionZero ? LogNormalDistribution.Draw(random, 0, 1) : 0,
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
            string concentrationUnitString,
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
                    };
                })
                .ToList();
        }
    }
}
