using System.Reflection;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.HumanMonitoringAnalysis;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
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
            var targetUnit = ConcentrationUnit.ugPerL;
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator.FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, targetUnit);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.HumanMonitoringSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.TargetMatrix = samplingMethod.BiologicalMatrix;

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                CorrectedRelativePotencyFactors = rpfs,
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
            var targetUnit = ConcentrationUnit.ugPerL;
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, targetUnit);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.HumanMonitoringSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.TargetMatrix = samplingMethod.BiologicalMatrix;
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                CorrectedRelativePotencyFactors = rpfs,
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
        [DataRow(NonDetectsHandlingMethod.ReplaceByLODLOQSystem)]
        [DataRow(NonDetectsHandlingMethod.ReplaceByZeroLOQSystem)]
        public void HumanMonitoringAnalysisActionCalculator_TestChronicImpute1(NonDetectsHandlingMethod nonDetectsHandlingMethod) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = ConcentrationUnit.ugPerL;
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, targetUnit);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.HumanMonitoringSettings.MissingValueImputationMethod = MissingValueImputationMethod.ImputeFromData;
            project.HumanMonitoringSettings.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            project.HumanMonitoringSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.NonDetectImputationMethod = NonDetectImputationMethod.CensoredLogNormal;
            project.HumanMonitoringSettings.TargetMatrix = samplingMethod.BiologicalMatrix;
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicImpute_{nonDetectsHandlingMethod}");
            var result = calculator.Run(data, new CompositeProgressState());
        }

        /// <summary>
        /// Runs the HumanMonitoringAnalysis action: 
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestChronicImputeNDandMV(
            MissingValueImputationMethod missingValueImputationMethod,
            NonDetectImputationMethod nonDetectImputationMethod,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            bool hbmConvertToSingleTargetMatrix
        ) {
            var (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections) = generateHBMData();
            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.HumanMonitoringSettings.MissingValueImputationMethod = missingValueImputationMethod;
            project.HumanMonitoringSettings.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            project.HumanMonitoringSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.NonDetectImputationMethod = nonDetectImputationMethod;
            project.HumanMonitoringSettings.HbmConvertToSingleTargetMatrix = hbmConvertToSingleTargetMatrix;
            project.HumanMonitoringSettings.TargetMatrix = samplingMethodBlood.BiologicalMatrix;
            if (hbmConvertToSingleTargetMatrix) {
                project.HumanMonitoringSettings.ApplyKineticConversions = true;
            }

            //Target = Blood is unstandardised, Urine is standardised
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);
            var kineticConversionFactorModel = FakeHbmDataGenerator.FakeKineticConversionFactorModel(
                BiologicalMatrix.Blood,
                BiologicalMatrix.Urine, 
                substances.First()
            );

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethodBlood },
                KineticConversionFactorModels = new List<KineticConversionFactorModel> { kineticConversionFactorModel }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var result = calculator.Run(data, new CompositeProgressState());
            var hbmResults = result as HumanMonitoringAnalysisActionResult;
            calculator.UpdateSimulationData(data, hbmResults);
            var section = new HbmIndividualDistributionBySubstanceSection();
            section.Summarize(
                data.HbmIndividualCollections,
                substances,
                2.5,
                97.5,
                false
            );

            var multiplier = nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem ? 0 : 1;
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
                    concentration: 0.05 * multiplier,
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
                    concentration: 0.05 * multiplier,
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

            if (hbmConvertToSingleTargetMatrix) {
                if (missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    //for Subst 2 alle missing values are replaced by zero, therefor no positives available
                    Assert.IsTrue(section.IndividualRecords.Count > 0);
                } else {
                    //for Subst 2 all missing values are replaced by data = MV,
                    //therefor all samples are replaced from matrix conversion on the second sampling method
                    Assert.IsTrue(section.IndividualRecords.Count > 0);
                }
            } else {
                Assert.IsTrue(section.IndividualRecords.Count > 0);
            }
            if (nonDetectImputationMethod == NonDetectImputationMethod.CensoredLogNormal) {
                Assert.IsNotNull(hbmResults.HbmConcentrationModels);
                Assert.IsTrue(hbmResults.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.CensoredLogNormal) > 0);
                Assert.IsTrue(hbmResults.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.Empirical) > 0);
                Assert.IsTrue((hbmResults.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Mu > 0);
                Assert.IsTrue((hbmResults.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Sigma > 0);
                Assert.AreEqual(meanSubst1Cens, section.IndividualRecords[1].MeanAll, 1e-3);

            } else {
                Assert.IsNull(hbmResults.HbmConcentrationModels);
                if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0LOD, section.IndividualRecords[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1LOD, section.IndividualRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0LOD, section.IndividualRecords[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1LOD, section.IndividualRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0Zero, section.IndividualRecords[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1Zero, section.IndividualRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(meanSubst1LOD, section.IndividualRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(meanSubst1LOD, section.IndividualRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(meanSubst1Zero, section.IndividualRecords[1].MeanAll, 1e-5);
                }
            }
        }

        /// <summary>
        /// Runs the HumanMonitoringAnalysis action: 
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, true)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.SetZero, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZero, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.ReplaceByLimit, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByLODLOQSystem, false)]
        [DataRow(MissingValueImputationMethod.ImputeFromData, NonDetectImputationMethod.CensoredLogNormal, NonDetectsHandlingMethod.ReplaceByZeroLOQSystem, false)]
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestAcuteImputeNDandMV(
            MissingValueImputationMethod missingValueImputationMethod,
            NonDetectImputationMethod nonDetectImputationMethod,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            bool hbmConvertToSingleTargetMatrix
        ) {
            var (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections) = generateHBMData();
            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.HumanMonitoringSettings.MissingValueImputationMethod = missingValueImputationMethod;
            project.HumanMonitoringSettings.NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            project.HumanMonitoringSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.NonDetectImputationMethod = nonDetectImputationMethod;
            project.HumanMonitoringSettings.HbmConvertToSingleTargetMatrix = hbmConvertToSingleTargetMatrix;
            project.HumanMonitoringSettings.TargetMatrix = samplingMethodBlood.BiologicalMatrix;
            if (hbmConvertToSingleTargetMatrix) {
                project.HumanMonitoringSettings.ApplyKineticConversions = true;
            }

            //Target = Blood is unstandardised, Urine is standardised
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);
            var kineticConversionFactorModel = FakeHbmDataGenerator
                .FakeKineticConversionFactorModel(
                    BiologicalMatrix.Blood,
                    BiologicalMatrix.Urine,
                    substances.First()
                );

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethodBlood },
                KineticConversionFactorModels = new List<KineticConversionFactorModel> { kineticConversionFactorModel }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var result = calculator.Run(data, new CompositeProgressState()) as HumanMonitoringAnalysisActionResult;
            calculator.UpdateSimulationData(data, result);

            var section = new HbmIndividualDayDistributionBySubstanceSection();
            section.Summarize(
                data.HbmIndividualDayCollections,
                substances,
                2.5,
                97.5,
                false
            );
            var multiplier = nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem ? 0 : 1;
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
                    concentration: 0.05 * multiplier,
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
                    concentration: 0.05 * multiplier,
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

            if (hbmConvertToSingleTargetMatrix) {
                if (missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    //for Subst 2 all missing values are replaced by zero, therefor no positives available
                    Assert.IsTrue(section.IndividualDayRecords.Count > 0);
                } else {
                    //for Subst 2 all missing values are replaced by data = MV,
                    //therefor all samples are replaced from matrix conversion on the second sampkling method
                    Assert.IsTrue(section.IndividualDayRecords.Count > 0);
                }
            } else {
                Assert.IsTrue(section.IndividualDayRecords.Count > 0);
            }
            if (nonDetectImputationMethod == NonDetectImputationMethod.CensoredLogNormal) {
                Assert.IsNotNull(result.HbmConcentrationModels);
                Assert.IsTrue(result.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.CensoredLogNormal) > 0);
                Assert.IsTrue(result.HbmConcentrationModels.Count(c => c.Value.ModelType == ConcentrationModelType.Empirical) > 0);
                Assert.IsTrue((result.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Mu > 0);
                Assert.IsTrue((result.HbmConcentrationModels.First().Value as CMCensoredLogNormal).Sigma > 0);
                Assert.AreEqual(meanSubst1Cens, section.IndividualDayRecords[1].MeanAll, 1e-3);

            } else {
                Assert.IsNull(result.HbmConcentrationModels);
                if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0LOD, section.IndividualDayRecords[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1LOD, section.IndividualDayRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0LOD, section.IndividualDayRecords[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1LOD, section.IndividualDayRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                    Assert.AreEqual(meanSubst0Zero, section.IndividualDayRecords[0].MeanAll, 1e-5);
                    Assert.AreEqual(meanSubst1Zero, section.IndividualDayRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(meanSubst1LOD, section.IndividualDayRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(meanSubst1LOD, section.IndividualDayRecords[1].MeanAll, 1e-5);
                } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero && missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                    Assert.AreEqual(meanSubst1Zero, section.IndividualDayRecords[1].MeanAll, 1e-5);
                }
            }
        }

        /// <summary>
        /// Hbm analysis should impute missing body weight by the average body weight.
        /// </summary>
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_MissingBodyWeight_ShouldImputeWithAverageBodyWeight() {
            var randomSamplingWeights = new McraRandomGenerator(seed: 1);
            var randomBodyWeights = new McraRandomGenerator(seed: 2);
            var individuals = MockIndividualsGenerator.Create(25, 2, randomSamplingWeights, useSamplingWeights: true, null, randomBodyWeights);

            // Add some missing body weights as NaN
            individuals = individuals.Select((i, ix) => {
                if (ix % 4 == 0) {
                    i.BodyWeight = double.NaN;
                }
                return i;
            }).ToList();

            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, MethodBase.GetCurrentMethod().Name);

            var hbmAnalysisActionResult = header.Item2 as HumanMonitoringAnalysisActionResult;

            Assert.IsFalse(hbmAnalysisActionResult.HbmIndividualDayConcentrations.All(c => c.HbmIndividualDayConcentrations.All(v => double.IsNaN(v.SimulatedIndividualBodyWeight))));
            Assert.IsFalse(hbmAnalysisActionResult.HbmIndividualConcentrations.All(c => c.HbmIndividualConcentrations.All(v => double.IsNaN(v.SimulatedIndividualBodyWeight))));
            var avgBwFromIndividuals = individuals
                .Where(i => !double.IsNaN(i.BodyWeight))
                .Select(i => i.BodyWeight)
                .Average();
            var avgBwFromHbmData = hbmAnalysisActionResult.HbmIndividualDayConcentrations
                .SelectMany(d => d.HbmIndividualDayConcentrations)
                .DistinctBy(i => i.Individual)
                .Select(d => d.SimulatedIndividualBodyWeight)
                .Average();
            Assert.AreEqual(avgBwFromIndividuals, avgBwFromHbmData, 0.00000001);
        }

        /// <summary>
        /// Runs the HumanMonitoringAnalysis action:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// Checks cumulative exposure target and compares it with a calculation based on the samples.
        /// The target is Blood, Urine samples are converted to Blood.
        /// Blood is unstandardised, Urine is standardised for specific gravity and creatinine.
        /// Kinetic conversion factors are specified according to these scenarios.
        /// </summary>
        [DataRow(false, StandardiseUrineMethod.SpecificGravity)]
        [DataRow(true, StandardiseUrineMethod.SpecificGravity)]
        [DataRow(true, StandardiseUrineMethod.CreatinineStandardisation)]
        [DataRow(false, StandardiseUrineMethod.SpecificGravity)]
        [DataRow(true, StandardiseUrineMethod.SpecificGravity)]
        [DataRow(true, StandardiseUrineMethod.CreatinineStandardisation)]
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestAcuteCumulative_StandardisedUrine(
            bool standardiseUrine,
            StandardiseUrineMethod standardiseUrineMethod
        ) {
            var (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections) = generateSimpleHBMData();
            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.HumanMonitoringSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.ApplyKineticConversions = true;
            project.HumanMonitoringSettings.HbmConvertToSingleTargetMatrix = true;
            project.HumanMonitoringSettings.TargetMatrix = samplingMethodBlood.BiologicalMatrix;
            project.HumanMonitoringSettings.StandardiseUrine = standardiseUrine;
            project.HumanMonitoringSettings.StandardiseUrineMethod = standardiseUrineMethod;
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);
            var kineticConversionFactor = FakeHbmDataGenerator
                .FakeKineticConversionFactor(
                    BiologicalMatrix.Urine,
                    BiologicalMatrix.Blood,
                    substances[2]
                );

            //Target = Blood is unstandardised, Urine is standardised
            kineticConversionFactor.DoseUnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL);
            if (standardiseUrine) {
                if (standardiseUrineMethod == StandardiseUrineMethod.SpecificGravity) {
                    kineticConversionFactor.DoseUnitFrom = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL);
                    kineticConversionFactor.ExpressionTypeFrom = ExpressionType.SpecificGravity;
                }
                if (standardiseUrineMethod == StandardiseUrineMethod.CreatinineStandardisation) {
                    kineticConversionFactor.DoseUnitFrom = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerg);
                    kineticConversionFactor.ExpressionTypeFrom = ExpressionType.Creatinine;
                }
            } else {
                kineticConversionFactor.DoseUnitFrom = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL);
            }
            var kineticConversionFactorModel = KineticConversionFactorCalculatorFactory.Create(
                conversion: kineticConversionFactor,
                useSubgroups: false
            );

            substances.ForEach(c => c.IsLipidSoluble = true);
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethodBlood },
                KineticConversionFactorModels = new List<KineticConversionFactorModel> { kineticConversionFactorModel }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var result = calculator.Run(data, new CompositeProgressState()) as HumanMonitoringAnalysisActionResult;
            var actualCumulative = result.HbmCumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations.Sum(c => c.CumulativeConcentration);

            var targetSamples = hbmSampleSubstanceCollections
                .SingleOrDefault(c => c.SamplingMethod == samplingMethodBlood)
                .HumanMonitoringSampleSubstanceRecords
                .SelectMany(c => c.HumanMonitoringSampleSubstances, (s, sc) => (
                    sample: s.HumanMonitoringSample,
                    sampleSubstance: sc
                ))
                .ToList();

            var otherSamples = hbmSampleSubstanceCollections
               .SingleOrDefault(c => c.SamplingMethod != samplingMethodBlood)
               .HumanMonitoringSampleSubstanceRecords
               .SelectMany(c => c.HumanMonitoringSampleSubstances, (s, sc) => (
                    sample: s.HumanMonitoringSample,
                    sampleSubstance: sc
                ))
                .ToList();

            var targetSubstances = targetSamples.Select(c => c.sampleSubstance.Key).ToList();
            var otherSubstances = otherSamples.Select(c => c.sampleSubstance.Key).ToList();
            var convertedSubstances = otherSubstances.Except(targetSubstances).ToList();
            var expectedCumulativeOther = double.NaN;
            var expectedCumulativeTarget = targetSamples.Sum(c => c.sampleSubstance.Value.Residue);

            if (standardiseUrine) {
                if (standardiseUrineMethod == StandardiseUrineMethod.SpecificGravity) {
                    expectedCumulativeOther = otherSamples
                        .Where(c => convertedSubstances.Contains(c.sampleSubstance.Key))
                        .Sum(c => c.sampleSubstance.Value.Residue * (1.024 - 1) / (c.sample.SpecificGravity.Value - 1) * kineticConversionFactor.ConversionFactor);
                }
                if (standardiseUrineMethod == StandardiseUrineMethod.CreatinineStandardisation) {
                    expectedCumulativeOther = otherSamples
                        .Where(c => convertedSubstances.Contains(c.sampleSubstance.Key))
                        .Sum(c => c.sampleSubstance.Value.Residue / c.sample.Creatinine.Value * 100 * kineticConversionFactor.ConversionFactor);
                }
            } else {
                expectedCumulativeOther = otherSamples
                    .Where(c => convertedSubstances.Contains(c.sampleSubstance.Key))
                    .Sum(c => c.sampleSubstance.Value.Residue * kineticConversionFactor.ConversionFactor);
            }

            Assert.AreEqual((expectedCumulativeTarget + expectedCumulativeOther), actualCumulative, 1e-6);
        }

        /// <summary>
        /// Runs the HumanMonitoringAnalysis action:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// Checks cumulative exposure target and compares it with a calculation based on the samples.
        /// The target is Blood, Urine samples are converted to Blood.
        /// Blood is standardised for gravimetric analysis and enzymatic summation, Urine is unstandardised.
        /// Kinetic conversion factors are specified according to these scenarios.
        /// </summary>
        [DataRow(false, StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(true, StandardiseBloodMethod.EnzymaticSummation)]
        [DataRow(true, StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(false, StandardiseBloodMethod.GravimetricAnalysis)]
        [DataRow(true, StandardiseBloodMethod.EnzymaticSummation)]
        [DataRow(true, StandardiseBloodMethod.GravimetricAnalysis)]
        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_TestAcuteCumulative_StandardisedBlood(
            bool standardiseBlood,
            StandardiseBloodMethod standardiseBloodMethod
        ) {
            var (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections) = generateSimpleHBMData();
            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.HumanMonitoringSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.ApplyKineticConversions = true;
            project.HumanMonitoringSettings.HbmConvertToSingleTargetMatrix = true;
            project.HumanMonitoringSettings.TargetMatrix = samplingMethodBlood.BiologicalMatrix;
            project.HumanMonitoringSettings.StandardiseBlood = standardiseBlood;
            project.HumanMonitoringSettings.StandardiseBloodMethod = standardiseBloodMethod;
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine);
            var kineticConversionFactor = FakeHbmDataGenerator
                .FakeKineticConversionFactor(
                    BiologicalMatrix.Urine,
                    BiologicalMatrix.Blood,
                    substances[2]
                );
            kineticConversionFactor.DoseUnitFrom = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL);
            //Target = Blood is standardised, Urine is unstandardised
            if (standardiseBlood) {
                kineticConversionFactor.DoseUnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerg);
                kineticConversionFactor.ExpressionTypeTo = ExpressionType.Lipids;
            } else {
                kineticConversionFactor.DoseUnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL);
            }
            var kineticConversionFactorModel = KineticConversionFactorCalculatorFactory.Create(
                conversion: kineticConversionFactor,
                useSubgroups: false
            );
            substances.ForEach(c => c.IsLipidSoluble = true);
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethodBlood },
                KineticConversionFactorModels = new List<KineticConversionFactorModel> { kineticConversionFactorModel }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var result = calculator.Run(data, new CompositeProgressState()) as HumanMonitoringAnalysisActionResult;
            var actualCumulative = result.HbmCumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations.Sum(c => c.CumulativeConcentration);

            var targetSamples = hbmSampleSubstanceCollections
                .SingleOrDefault(c => c.SamplingMethod == samplingMethodBlood)
                .HumanMonitoringSampleSubstanceRecords
                .SelectMany(c => c.HumanMonitoringSampleSubstances, (s, sc) => (
                    sample: s.HumanMonitoringSample,
                    sampleSubstance: sc
                ))
                .ToList();

            var otherSamples = hbmSampleSubstanceCollections
               .SingleOrDefault(c => c.SamplingMethod != samplingMethodBlood)
               .HumanMonitoringSampleSubstanceRecords
               .SelectMany(c => c.HumanMonitoringSampleSubstances, (s, sc) => (
                    sample: s.HumanMonitoringSample,
                    sampleSubstance: sc
                ))
                .ToList();

            var targetSubstances = targetSamples.Select(c => c.sampleSubstance.Key).ToList();
            var otherSubstances = otherSamples.Select(c => c.sampleSubstance.Key).ToList();
            var convertedSubstances = otherSubstances.Except(targetSubstances).ToList();
            var expectedCumulativeTarget = double.NaN;
            var expectedCumulativeOther = double.NaN;

            if (standardiseBlood) {
                if (standardiseBloodMethod == StandardiseBloodMethod.GravimetricAnalysis) {
                    expectedCumulativeTarget = targetSamples
                        .Sum(c => c.sampleSubstance.Value.Residue / c.sample.LipidGrav.Value * 100);
                    expectedCumulativeOther = otherSamples
                        .Where(c => convertedSubstances.Contains(c.sampleSubstance.Key))
                        .Sum(c => c.sampleSubstance.Value.Residue * kineticConversionFactor.ConversionFactor);
                }
                if (standardiseBloodMethod == StandardiseBloodMethod.EnzymaticSummation) {
                    expectedCumulativeTarget = targetSamples
                        .Sum(c => c.sampleSubstance.Value.Residue / c.sample.LipidEnz.Value * 100);
                    expectedCumulativeOther = otherSamples
                        .Where(c => convertedSubstances.Contains(c.sampleSubstance.Key))
                        .Sum(c => c.sampleSubstance.Value.Residue * kineticConversionFactor.ConversionFactor);
                }
            } else {
                expectedCumulativeTarget = targetSamples
                    .Sum(c => c.sampleSubstance.Value.Residue);
                expectedCumulativeOther = otherSamples
                    .Where(c => convertedSubstances.Contains(c.sampleSubstance.Key))
                    .Sum(c => c.sampleSubstance.Value.Residue * kineticConversionFactor.ConversionFactor);
            }

            Assert.AreEqual((expectedCumulativeTarget + expectedCumulativeOther), actualCumulative, 1e-6);
        }

        [TestMethod]
        public void HumanMonitoringAnalysisActionCalculator_FilterActiveSubstances_ShouldOnlyIncludeSamplesFromActiveSubstances() {
            var randomSamplingWeights = new McraRandomGenerator(seed: 1);
            var randomBodyWeights = new McraRandomGenerator(seed: 2);
            var individuals = MockIndividualsGenerator.Create(25, 2, randomSamplingWeights, useSamplingWeights: true, null, randomBodyWeights);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(5);
            var activeSubstances = substances.Take(3).ToList();
            var nonActiveSubstances = substances.TakeLast(2).ToList();
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod, ConcentrationUnit.ugPerL);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = activeSubstances,
                HbmSampleSubstanceCollections = hbmSampleSubstanceCollections,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod }
            };

            var calculator = new HumanMonitoringAnalysisActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, MethodBase.GetCurrentMethod().Name);

            var hbmAnalysisActionResult = header.Item2 as HumanMonitoringAnalysisActionResult;

            Assert.IsTrue(hbmAnalysisActionResult
                .HbmIndividualDayConcentrations
                .All(c => c.HbmIndividualDayConcentrations
                    .All(v => v.ConcentrationsBySubstance.All(c => activeSubstances.Contains(c.Key)))));
            Assert.IsFalse(hbmAnalysisActionResult
               .HbmIndividualDayConcentrations
               .All(c => c.HbmIndividualDayConcentrations
                   .All(v => v.ConcentrationsBySubstance.All(c => nonActiveSubstances.Contains(c.Key)))));
            Assert.IsTrue(hbmAnalysisActionResult
               .HbmIndividualConcentrations
               .All(c => c.HbmIndividualConcentrations
                   .All(v => v.ConcentrationsBySubstance.All(c => activeSubstances.Contains(c.Key)))));
            Assert.IsFalse(hbmAnalysisActionResult
                .HbmIndividualConcentrations
                .All(c => c.HbmIndividualConcentrations
                    .All(v => v.ConcentrationsBySubstance.All(c => nonActiveSubstances.Contains(c.Key)))));
        }

        /// <summary>
        /// Simple generation of data, currently one individual with one day is simulated, 
        /// but more individuals and more days is also allowed.
        /// Specify 3 substance and assign substance 0 an 1 to the target matric, substance 1 and 2 to the other matrix.
        /// This means that substance 2 has to be converted.
        /// </summary>
        /// <returns></returns>
        private (List<Compound>, Dictionary<Compound, double>, HumanMonitoringSamplingMethod, List<HumanMonitoringSample>, List<HumanMonitoringSampleSubstanceCollection>) generateSimpleHBMData(int seed = 1) {
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(1, 1, random, useSamplingWeights: false);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var samplingMethodBlood = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood, "Serum");
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine, "Spot");

            var substancesBlood = new List<Compound>() { substances[0], substances[1] };
            var substancesUrine = new List<Compound>() { substances[1], substances[2] };
            var hbmSamplesBlood = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesBlood, samplingMethodBlood);
            var hbmSamplesUrine = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesUrine, samplingMethodUrine);
            var survey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);
            var hbmSampleSubstanceCollectionsBlood = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(substancesBlood, hbmSamplesBlood, survey, new());
            var hbmSampleSubstanceCollectionsUrine = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(substancesUrine, hbmSamplesUrine, survey, new());
            var hbmSampleSubstanceCollections = new List<HumanMonitoringSampleSubstanceCollection>() {
                hbmSampleSubstanceCollectionsBlood[0],
                hbmSampleSubstanceCollectionsUrine[0]
            };
            return (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections);
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
            var samplingMethodBlood = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Blood, "Serum");
            var samplingMethodUrine = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(BiologicalMatrix.Urine, "Spot");

            var substancesBlood = new List<Compound>() { substances[0], substances[1], substances[2] };
            var substancesUrine = new List<Compound>() { substances[2], substances[3], substances[4] };
            var hbmSamplesBlood = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesBlood, samplingMethodBlood);
            var hbmSamplesUrine = FakeHbmDataGenerator.FakeHbmSamples(individualDays, substancesUrine, samplingMethodUrine);
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
            var survey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);
            var hbmSampleSubstanceCollectionsBlood = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(substancesBlood, hbmSamplesBlood, survey, new());
            var hbmSampleSubstanceCollectionsUrine = HumanMonitoringSampleSubstanceCollectionsBuilder.Create(substancesUrine, hbmSamplesUrine, survey, new());
            var hbmSampleSubstanceCollections = new List<HumanMonitoringSampleSubstanceCollection>() {
                hbmSampleSubstanceCollectionsBlood[0],
                hbmSampleSubstanceCollectionsUrine[0]
            };
            return (substances, rpfs, samplingMethodBlood, hbmSamplesBlood, hbmSampleSubstanceCollections);
        }
    }
}
