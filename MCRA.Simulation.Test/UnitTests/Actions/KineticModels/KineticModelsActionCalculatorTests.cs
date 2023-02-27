using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.KineticModels;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the KineticModels action
    /// </summary>
    [TestClass]
    public class KineticModelsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the KineticModels action: load data, load default data, summarize 
        /// action result, load data uncertain method
        /// </summary>
        [TestMethod]
        public void KineticModelsActionCalculator_TestLoadAbsorptionFactors() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var exposureRoutes = new List<ExposureRouteType>() { 
                ExposureRouteType.Dietary, 
                ExposureRouteType.Dermal, 
                ExposureRouteType.Inhalation, 
                ExposureRouteType.Oral 
            };
            var substances = MockSubstancesGenerator.Create(1);
            var referenceCompound = substances.First();
            var absorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances);
            var kineticAbsorptionFactors = new List<KineticAbsorptionFactor>();
            foreach (var item in absorptionFactors) {
                kineticAbsorptionFactors.Add(new KineticAbsorptionFactor() {
                    AbsorptionFactor = item.Value,
                    Compound = item.Key.Substance,
                    RouteTypeString = item.Key.RouteType.ToString(),
                });
            }

            var compiledData = new CompiledData() {
                AllKineticAbsorptionFactors = kineticAbsorptionFactors.ToList(),
            };

            var project = new ProjectDto();
            project.AssessmentSettings.Aggregate = true;
            project.KineticModelSettings.CodeModel = InternalModelType.AbsorptionFactorModel.ToString();

            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceCompound = referenceCompound,
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var calculator = new KineticModelsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsTrue(data.AbsorptionFactors.Any());
            Assert.IsTrue(data.KineticAbsorptionFactors.Any());
            Assert.IsNull(data.KineticModelInstances);

            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = new List<UncertaintySource>() { UncertaintySource.KineticModelParameters }
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.KineticModelParameters] = random
            };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Runs the KineticModels action: load data, load default data, summarize 
        /// action result, load data uncertain method
        /// </summary>
        [TestMethod]
        public void KineticModelsActionCalculator_TestLoadKineticModelInstance() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var exposureRoutes = new List<ExposureRouteType>() {
                ExposureRouteType.Dietary,
                ExposureRouteType.Dermal,
                ExposureRouteType.Inhalation,
                ExposureRouteType.Oral
            };
            var substances = MockSubstancesGenerator.Create(1);
            var referenceCompound = substances.First();
            var kineticModelinstance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv6KineticModelInstance(referenceCompound);
            var kineticModelInstances = new List<KineticModelInstance>() { kineticModelinstance };

            var compiledData = new CompiledData() {
                AllKineticModelInstances = kineticModelInstances.ToList(),
            };

            var project = new ProjectDto();
            project.AssessmentSettings.Aggregate = true;
            project.KineticModelSettings.InternalModelType = InternalModelType.PBKModel;
            project.KineticModelSettings.CodeModel = kineticModelinstance.IdModelDefinition;
            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceCompound = referenceCompound,
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var calculator = new KineticModelsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsTrue(data.KineticModelInstances.Any());
            Assert.AreEqual(4, data.AbsorptionFactors.Count);
            Assert.IsNull(data.KineticAbsorptionFactors);
            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = new List<UncertaintySource>() { UncertaintySource.KineticModelParameters }
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.KineticModelParameters] = random
            };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }
    }
}