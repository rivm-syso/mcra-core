using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Actions.AOPNetworks;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the AOPNetworks action
    /// </summary>
    [TestClass]
    public class AOPNetworksActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the AOP Networks module: load data and summarize method
        /// project.EffectSettings.RestrictAopByFocalUpstreamEffect = true;
        /// project.EffectSettings.CodeAopNetwork = aop.Code;
        /// </summary>
        [TestMethod]
        public void AOPNetworksActionCalculator_Test1() {
            var effects = MockEffectsGenerator.Create(5);
            var aop = new AdverseOutcomePathwayNetwork() {
                AdverseOutcome = effects.First(),
                Code = "network",
                Description = "Description",
                Name = "Name",
                Reference = "Reference",
                RiskTypeString = "RiskTypeString"
            };
            var allAdverseOutcomePathwayNetworks = new Dictionary<string, AdverseOutcomePathwayNetwork>();
            var effectRelations0 = new List<EffectRelationship> {
                new EffectRelationship() {
                    AdverseOutcomePathwayNetwork = aop,
                    DownstreamKeyEvent = effects[0],
                    UpstreamKeyEvent = effects[1],
                }
            };
            aop.EffectRelations = effectRelations0;
            allAdverseOutcomePathwayNetworks["network"] = aop;

            var compiledData = new CompiledData() {
                AllAdverseOutcomePathwayNetworks = allAdverseOutcomePathwayNetworks,
            };

            var config = new AOPNetworksModuleConfig {
                RestrictAopByFocalUpstreamEffect = true,
                CodeAopNetwork = aop.Code
            };
            var project = new ProjectDto();
            project.SaveModuleConfiguration(config);

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                AllEffects = effects,
                SelectedEffect = effects.First()
            };
            var calculator = new AOPNetworksActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.RelevantEffects);
            Assert.AreEqual(2, data.RelevantEffects.Count); 
            Assert.IsNotNull(data.AdverseOutcomePathwayNetwork);
        }

        /// <summary>
        /// Runs the AOP Networks action: load data and summarize method, 
        /// project.EffectSettings.RestrictAopByFocalUpstreamEffect = false;
        /// project.EffectSettings.CodeAopNetwork = aop.Code;
        /// </summary>
        [TestMethod]
        public void AOPNetworksActionCalculator_Test2() {
            var effects = MockEffectsGenerator.Create(5);
            var aop = new AdverseOutcomePathwayNetwork() {
                AdverseOutcome = effects.First(),
                Code = "network",
                Description = "Description",
                Name = "Name",
                Reference = "Reference",
                RiskTypeString = "RiskTypeString"
            };
            var allAdverseOutcomePathwayNetworks = new Dictionary<string, AdverseOutcomePathwayNetwork>();
            var effectRelations0 = new List<EffectRelationship> {
                new EffectRelationship() {
                    AdverseOutcomePathwayNetwork = aop,
                    DownstreamKeyEvent = effects[0],
                    UpstreamKeyEvent = effects[1],
                }
            };
            aop.EffectRelations = effectRelations0;
            allAdverseOutcomePathwayNetworks["network"] = aop;

            var compiledData = new CompiledData() {
                AllAdverseOutcomePathwayNetworks = allAdverseOutcomePathwayNetworks,
            };

            var config = new AOPNetworksModuleConfig {
                RestrictAopByFocalUpstreamEffect = false,
                CodeAopNetwork = aop.Code
            };
            var project = new ProjectDto(config);

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData() {
                AllEffects = effects,
                SelectedEffect = effects.First()
            };

            var calculator = new AOPNetworksActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad2");
            Assert.IsNotNull(data.RelevantEffects);
            Assert.AreEqual(2, data.RelevantEffects.Count);
            Assert.IsNotNull(data.AdverseOutcomePathwayNetwork);
        }
    }
}