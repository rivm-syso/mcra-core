using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.HumanMonitoringAnalysis;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the HumanMonitoringAnalysis action
    /// </summary>
    [TestClass]
    public class HumanMonitoringAnalysisActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the HumanMonitoringAnalysis action:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.MixtureSelectionSettings.McrExposureApproachType = ExposureApproachType.ExposureBased;
            project.KineticModelSettings.BiologicalMatrix = samplingMethod.BiologicalMatrix;

            var data = new ActionData() {
                ActiveSubstances = substances,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                CorrectedRelativePotencyFactors = rpfs
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "TestAcute");
            var result = calculator.Run(data, new CompositeProgressState());
        }

        /// <summary>
        /// Runs the HumanMonitoringAnalysis action: 
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.MixtureSelectionSettings.McrExposureApproachType = ExposureApproachType.ExposureBased;
            project.KineticModelSettings.BiologicalMatrix = samplingMethod.BiologicalMatrix;
            var data = new ActionData() {
                ActiveSubstances = substances,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                CorrectedRelativePotencyFactors = rpfs
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "TestChronic");
            var result = calculator.Run(data, new CompositeProgressState());
        }

        /// <summary>
        /// Runs the HumanMonitoringAnalysis action: 
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestChronicImpute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.HumanMonitoringSettings.MissingValueImputationMethod = MissingValueImputationMethod.ImputeFromData;
            project.HumanMonitoringSettings.NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem;
            project.MixtureSelectionSettings.McrExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.NonDetectImputationMethod = NonDetectImputationMethod.CensoredLogNormal;
            project.KineticModelSettings.BiologicalMatrix = samplingMethod.BiologicalMatrix;
            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "TestChronicImpute");
            var result = calculator.Run(data, new CompositeProgressState());
        }


        /// <summary>
        /// Runs the HumanMonitoringAnalysis action: 
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestChronicImputeNDandMV(
            MissingValueImputationMethod missingValueImputationMethod,
            NonDetectImputationMethod nonDetectImputationMethod,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            bool imputeHbmConcentrationsFromOtherMatrices
        ) {
            var (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections) = generateHBMData();
            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.HumanMonitoringSettings.MissingValueImputationMethod = missingValueImputationMethod;
            project.HumanMonitoringSettings.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            project.MixtureSelectionSettings.McrExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.NonDetectImputationMethod = nonDetectImputationMethod;
            project.HumanMonitoringSettings.ImputeHbmConcentrationsFromOtherMatrices = imputeHbmConcentrationsFromOtherMatrices;

            project.KineticModelSettings.BiologicalMatrix = samplingMethodBlood.BiologicalMatrix;
            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethodBlood },
                HbmTargetConcentrationUnits = new List<TargetUnit> { new TargetUnit(ExposureUnit.ugPerKgBWPerDay) }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var result = calculator.Run(data, new CompositeProgressState());
            var hbmResults = result as HumanMonitoringAnalysisActionResult;
            calculator.UpdateSimulationData(data, hbmResults);
            var section = new HbmIndividualDistributionBySubstanceSection();
            section.Summarize(
                data.HbmIndividualConcentrations,
                substances,
                samplingMethodBlood.BiologicalMatrix,
                2.5,
                97.5
            );

            var samplesSubst1VAL = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[1]].ResType == ResType.VAL)
                .Select(c => (
                    concentration: c.SampleAnalyses.First().Concentrations[substances[1]].Concentration,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst1ND = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[1]].ResType == ResType.LOD)
                .Select(c => (
                    concentration: 0.05,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst0VAL = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[0]].ResType == ResType.VAL)
                .Select(c => (
                    concentration: c.SampleAnalyses.First().Concentrations[substances[0]].Concentration,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst0ND = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[0]].ResType == ResType.LOD)
                .Select(c => (
                    concentration: 0.05,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst0MV = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[0]].ResType == ResType.MV)
                .Select(c => (
                    concentration: double.NaN,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();

            var sumSubst0VAL = samplesSubst0VAL.Sum(c => c.concentration * c.samplingWeight);
            var sumSubst0ND = samplesSubst0ND.Sum(c => c.concentration * c.samplingWeight);
            var meanSubst0LOD = (double)(sumSubst0VAL + sumSubst0ND) / (samplesSubst0VAL.Sum(c => c.samplingWeight) + samplesSubst0ND.Sum(c => c.samplingWeight) + samplesSubst0MV.Sum(c => c.samplingWeight));
            var meanSubst0Zero = (double)sumSubst0VAL / (samplesSubst0VAL.Sum(c => c.samplingWeight) + samplesSubst0ND.Sum(c => c.samplingWeight) + samplesSubst0MV.Sum(c => c.samplingWeight));

            var sumSubst1VAL = samplesSubst1VAL.Sum(c => c.concentration * c.samplingWeight);
            var sumSubst1ND = samplesSubst1ND.Sum(c => c.concentration * c.samplingWeight);
            var meanSubst1LOD = (double)(sumSubst1VAL + sumSubst1ND) / (samplesSubst1VAL.Sum(c => c.samplingWeight) + samplesSubst1ND.Sum(c => c.samplingWeight));
            var meanSubst1Zero = (double)sumSubst1VAL / (samplesSubst1VAL.Sum(c => c.samplingWeight) + samplesSubst1ND.Sum(c => c.samplingWeight));
            var meanSubst1Cens = (double)(sumSubst1VAL + 2 * 0.0029690122151672564) / (samplesSubst1VAL.Sum(c => c.samplingWeight) + samplesSubst1ND.Sum(c => c.samplingWeight));


            Assert.AreEqual(48, samplesSubst1VAL.Count);
            Assert.AreEqual(2, samplesSubst1ND.Count);
            Assert.AreEqual(46, samplesSubst0VAL.Count);
            Assert.AreEqual(2, samplesSubst0ND.Count);
            if (imputeHbmConcentrationsFromOtherMatrices) {
                if (missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    //for Subst 2 alle missing values are replaced by zero, therefor no positives available
                    Assert.AreEqual(5, section.Records.Count);
                } else {
                    //for Subst 2 alle missing values are replaced by data = MV, therefor all samples are replaced from matrix conversion on the second sampkling method
                    Assert.AreEqual(5, section.Records.Count);
                }
            } else {
                Assert.AreEqual(2, section.Records.Count);
            }
            if (nonDetectImputationMethod == NonDetectImputationMethod.CensoredLogNormal) {
                Assert.IsNotNull(hbmResults.HbmConcentrationModels);
                Assert.AreEqual(2, hbmResults.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.CensoredLogNormal));
                Assert.AreEqual(4, hbmResults.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.Empirical));
                Assert.AreEqual(3.537, (hbmResults.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Mu, 1e-3);
                Assert.AreEqual(1.302, (hbmResults.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Sigma, 1e-3);
                Assert.AreEqual(meanSubst1Cens, section.Records[1].MeanAll, 1e-3);

            } else {
                Assert.IsNull(hbmResults.HbmConcentrationModels);
                if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0LOD, section.Records[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1LOD, section.Records[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0Zero, section.Records[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1Zero, section.Records[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(47, section.Records[0].MeanAll, 1);
                    Assert.AreEqual(meanSubst1LOD, section.Records[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(47, section.Records[0].MeanAll, 1);
                    Assert.AreEqual(meanSubst1Zero, section.Records[1].MeanAll, 1e-5);
                }
            }
        }

        /// <summary>
        /// Runs the HumanMonitoringAnalysis action: 
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestAcuteImputeNDandMV(
            MissingValueImputationMethod missingValueImputationMethod,
            NonDetectImputationMethod nonDetectImputationMethod,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            bool imputeHbmConcentrationsFromOtherMatrices
        ) {
            var (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections) = generateHBMData();
            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.HumanMonitoringSettings.MissingValueImputationMethod = missingValueImputationMethod;
            project.HumanMonitoringSettings.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            project.MixtureSelectionSettings.McrExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.NonDetectImputationMethod = nonDetectImputationMethod;
            project.HumanMonitoringSettings.ImputeHbmConcentrationsFromOtherMatrices = imputeHbmConcentrationsFromOtherMatrices;
            project.KineticModelSettings.BiologicalMatrix = samplingMethodBlood.BiologicalMatrix;
            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethodBlood },
                HbmTargetConcentrationUnits = new List<TargetUnit> { new TargetUnit(ExposureUnit.ugPerKgBWPerDay) }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var result = calculator.Run(data, new CompositeProgressState());
            var hbmResults = result as HumanMonitoringAnalysisActionResult;
            calculator.UpdateSimulationData(data, hbmResults);
            var section = new HbmIndividualDayDistributionBySubstanceSection();
            section.Summarize(
                data.HbmIndividualDayConcentrations,
                substances,
                samplingMethodBlood.BiologicalMatrix,
                2.5,
                97.5
            );

            var samplesSubst1VAL = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[1]].ResType == ResType.VAL)
                .Select(c => (
                    concentration: c.SampleAnalyses.First().Concentrations[substances[1]].Concentration,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst1ND = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[1]].ResType == ResType.LOD)
                .Select(c => (
                    concentration: 0.05,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst0VAL = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[0]].ResType == ResType.VAL)
                .Select(c => (
                    concentration: c.SampleAnalyses.First().Concentrations[substances[0]].Concentration,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst0ND = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[0]].ResType == ResType.LOD)
                .Select(c => (
                    concentration: 0.05,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();
            var samplesSubst0MV = hbmSamplesBlood
                .Where(c => c.SampleAnalyses.First().Concentrations[substances[0]].ResType == ResType.MV)
                .Select(c => (
                    concentration: double.NaN,
                    samplingWeight: c.Individual.SamplingWeight
                    )
                ).ToList();

            var sumSubst0VAL = samplesSubst0VAL.Sum(c => c.concentration * c.samplingWeight);
            var sumSubst0ND = samplesSubst0ND.Sum(c => c.concentration * c.samplingWeight);
            var meanSubst0LOD = (double)(sumSubst0VAL + sumSubst0ND) / (samplesSubst0VAL.Sum(c => c.samplingWeight) + samplesSubst0ND.Sum(c => c.samplingWeight) + samplesSubst0MV.Sum(c => c.samplingWeight));
            var meanSubst0Zero = (double)sumSubst0VAL / (samplesSubst0VAL.Sum(c => c.samplingWeight) + samplesSubst0ND.Sum(c => c.samplingWeight) + samplesSubst0MV.Sum(c => c.samplingWeight));

            var sumSubst1VAL = samplesSubst1VAL.Sum(c => c.concentration * c.samplingWeight);
            var sumSubst1ND = samplesSubst1ND.Sum(c => c.concentration * c.samplingWeight);
            var meanSubst1LOD = (double)(sumSubst1VAL + sumSubst1ND) / (samplesSubst1VAL.Sum(c => c.samplingWeight) + samplesSubst1ND.Sum(c => c.samplingWeight));
            var meanSubst1Zero = (double)sumSubst1VAL / (samplesSubst1VAL.Sum(c => c.samplingWeight) + samplesSubst1ND.Sum(c => c.samplingWeight));
            var meanSubst1Cens = (double)(sumSubst1VAL + 2 * 0.0029690122151672564) / (samplesSubst1VAL.Sum(c => c.samplingWeight) + samplesSubst1ND.Sum(c => c.samplingWeight));

            Assert.AreEqual(48, samplesSubst1VAL.Count);
            Assert.AreEqual(2, samplesSubst1ND.Count);
            Assert.AreEqual(46, samplesSubst0VAL.Count);
            Assert.AreEqual(2, samplesSubst0ND.Count);
            if (imputeHbmConcentrationsFromOtherMatrices) {
                if (missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    //for Subst 2 all missing values are replaced by zero, therefor no positives available
                    Assert.AreEqual(5, section.Records.Count);
                } else {
                    //for Subst 2 all missing values are replaced by data = MV, therefor all samples are replaced from matrix conversion on the second sampkling method
                    Assert.AreEqual(5, section.Records.Count);
                }
            } else {
                Assert.AreEqual(2, section.Records.Count);
            }
            if (nonDetectImputationMethod == NonDetectImputationMethod.CensoredLogNormal) {
                Assert.IsNotNull(hbmResults.HbmConcentrationModels);
                Assert.AreEqual(2, hbmResults.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.CensoredLogNormal));
                Assert.AreEqual(4, hbmResults.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.Empirical));
                Assert.AreEqual(3.537, (hbmResults.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Mu, 1e-3);
                Assert.AreEqual(1.302, (hbmResults.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Sigma, 1e-3);
                Assert.AreEqual(meanSubst1Cens, section.Records[1].MeanAll, 1e-3);

            } else {
                Assert.IsNull(hbmResults.HbmConcentrationModels);
                if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0LOD, section.Records[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1LOD, section.Records[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0Zero, section.Records[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1Zero, section.Records[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(47, section.Records[0].MeanAll, 1);
                    Assert.AreEqual(meanSubst1LOD, section.Records[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(47, section.Records[0].MeanAll, 1);
                    Assert.AreEqual(meanSubst1Zero, section.Records[1].MeanAll, 1e-5);
                }
            }
        }

        /// <summary>
        /// Blood contains substance 0, 1 and 2
        /// Blood samples substance 0: two samples missing, two samples nonDetect
        /// Blood samples substance 1: two samples nonDetect
        /// Blood samples substance 2: all missing
        /// Urine contains substance 2, 3 and 4
        /// Urine samples substance 2: all detects
        /// Urine samples substance 3: all detects
        /// Urine samples substance 4: one detect, all others nondetect
        /// </summary>
        /// <returns></returns>
        private (List<Compound>, Dictionary<Compound, double>, HumanMonitoringSamplingMethod, List<HumanMonitoringSample>, List<HumanMonitoringSampleSubstanceCollection>) generateHBMData(int seed = 1) {
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(5);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var samplingMethodBlood = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);

            var substancesBlood = new List<Compound>() { substances[0], substances[1], substances[2] };
            var substancesUrine = new List<Compound>() { substances[2], substances[3], substances[4] };
            var hbmSamplesBlood = FakeHbmDataGenerator.MockHumanMonitoringSamples(individualDays, substancesBlood, samplingMethodBlood);
            var hbmSamplesUrine = FakeHbmDataGenerator.MockHumanMonitoringSamples(individualDays, substancesUrine, samplingMethodUrine);
            var sampleSubst0_MV = new ConcentrationPerSample() {
                Compound = substances[0],
                Concentration = double.NaN,
                ResTypeString = ResType.MV.ToString()
            };
            var sampleSubst0_ND = new ConcentrationPerSample() {
                Compound = substances[0],
                Concentration = double.NaN,
                ResTypeString = ResType.LOD.ToString()
            };
            var sampleSubst1_ND = new ConcentrationPerSample() {
                Compound = substances[1],
                Concentration = double.NaN,
                ResTypeString = ResType.LOD.ToString()
            };
            var sampleSubst2_MV = new ConcentrationPerSample() {
                Compound = substances[2],
                Concentration = double.NaN,
                ResTypeString = ResType.MV.ToString()
            };
            var sampleSubst4_ND = new ConcentrationPerSample() {
                Compound = substances[4],
                Concentration = double.NaN,
                ResTypeString = ResType.LOD.ToString()
            };

            hbmSamplesBlood[0].SampleAnalyses.First().Concentrations[substances[0]] = sampleSubst0_MV;
            hbmSamplesBlood[1].SampleAnalyses.First().Concentrations[substances[0]] = sampleSubst0_MV;
            hbmSamplesBlood[2].SampleAnalyses.First().Concentrations[substances[0]] = sampleSubst0_ND;
            hbmSamplesBlood[3].SampleAnalyses.First().Concentrations[substances[0]] = sampleSubst0_ND;
            hbmSamplesBlood[0].SampleAnalyses.First().Concentrations[substances[1]] = sampleSubst1_ND;
            hbmSamplesBlood[1].SampleAnalyses.First().Concentrations[substances[1]] = sampleSubst1_ND;

            foreach (var sample in hbmSamplesBlood) {
                sample.SampleAnalyses.First().Concentrations[substances[2]] = sampleSubst2_MV;
            }

            for (int i = 1; i < individualDays.Count; i++) {
                hbmSamplesUrine[i].SampleAnalyses.First().Concentrations[substances[4]] = sampleSubst4_ND;
            }
            var survey = FakeHbmDataGenerator.MockHumanMonitoringSurvey(individualDays);
            var hbmSampleSubstanceCollectionsBlood = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(substancesBlood, hbmSamplesBlood, ConcentrationUnit.mgPerL, survey);
            var hbmSampleSubstanceCollectionsUrine = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(substancesUrine, hbmSamplesUrine, ConcentrationUnit.mgPerL, survey);
            var hbmSampleSubstanceCollections = new List<HumanMonitoringSampleSubstanceCollection>() { hbmSampleSubstanceCollectionsBlood[0], hbmSampleSubstanceCollectionsUrine[0] };
            return (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections);
        }
    }
}
