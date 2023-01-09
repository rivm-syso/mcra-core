using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.HumanMonitoringAnalysis;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using MCRA.Data.Compiled.Objects;

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
            var samplingMethod = MockHumanMonitoringDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = MockHumanMonitoringDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.MixtureSelectionSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.KineticModelSettings.CodeCompartment = samplingMethod.Compartment;

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
            var samplingMethod = MockHumanMonitoringDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = MockHumanMonitoringDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.MixtureSelectionSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.KineticModelSettings.CodeCompartment = samplingMethod.Compartment;
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
            var samplingMethod = MockHumanMonitoringDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmSampleSubstanceCollections = MockHumanMonitoringDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.HumanMonitoringSettings.MissingValueImputationMethod = MissingValueImputationMethod.ImputeFromData;
            project.HumanMonitoringSettings.NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLODLOQSystem;
            project.MixtureSelectionSettings.ExposureApproachType = ExposureApproachType.ExposureBased;
            project.HumanMonitoringSettings.NonDetectImputationMethod = NonDetectImputationMethod.CensoredLogNormal;
            project.KineticModelSettings.CodeCompartment = samplingMethod.Compartment;
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
    }
}
