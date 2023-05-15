using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.ModuleDefinitions;
using MCRA.Simulation.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Generic tests for checking the consistency of the module
    /// definitions with the action calculators.
    /// </summary>
    [TestClass]
    public class ActionCalculatorsTests {

        /// <summary>
        /// Runs ActionTiers: Tiers
        /// </summary>
        [TestMethod]
        public void ActionCalculators_TestTiers() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            foreach (var definition in definitions.Values) {
                var project = new ProjectDto();
                var calculator = ActionCalculatorProvider.Create(definition.ActionType, project, false);
                calculator.Verify();
                var settingsManager = calculator.GetSettingsManager();
                if (settingsManager != null) {
                    var tiers = definition.Tiers;
                    if (tiers?.Any() ?? false) {
                        foreach (var tier in tiers) {
                            settingsManager.SetTier(project, tier, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs ActionTiers: TierDisplayNames
        /// </summary>
        [TestMethod]
        public void ActionCalculators_TestTierDisplayNames() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            foreach (var definition in definitions.Values) {
                var project = new ProjectDto();
                var calculator = ActionCalculatorProvider.Create(definition.ActionType, project, false);
                calculator.Verify();
                var settingsManager = calculator.GetSettingsManager();
                if (settingsManager != null) {
                    var tiers = definition.Tiers;
                    if (tiers?.Any() ?? false) {
                        var tierEnumDisplayNames = settingsManager.GetAvailableTiers();
                        foreach (var tier in tiers) {
                            Assert.AreEqual(tierEnumDisplayNames[tier.Id], tier.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test whether the uncertainty sources specified by the action calculators
        /// are also included in the module definition.
        /// </summary>
        [TestMethod]
        public void ActionCalculators_TestUncertaintySources() {
            var definitionsInstance = McraModuleDefinitions.Instance;
            var definitions = definitionsInstance.ModuleDefinitions;
            foreach (var definition in definitions.Values) {

                // Make sure uncertainty settings are set
                var project = new ProjectDto();
                project.UncertaintyAnalysisSettings.ReSampleProcessingFactors = true;
                project.UncertaintyAnalysisSettings.RecomputeOccurrencePatterns = true;
                project.UncertaintyAnalysisSettings.ReSampleProcessingFactors = true;
                project.UncertaintyAnalysisSettings.ReSampleConcentrations = true;
                project.UncertaintyAnalysisSettings.ResampleIndividuals = true;
                project.UncertaintyAnalysisSettings.ReSamplePortions = true;
                project.UncertaintyAnalysisSettings.ReSampleImputationExposureDistributions = true;
                project.UncertaintyAnalysisSettings.ReSampleNonDietaryExposures = true;
                project.UncertaintyAnalysisSettings.ReSampleInterspecies = true;
                project.UncertaintyAnalysisSettings.ReSampleIntraSpecies = true;
                project.UncertaintyAnalysisSettings.ReSampleAssessmentGroupMemberships = true;
                project.UncertaintyAnalysisSettings.ResampleKineticModelParameters = true;
                project.UncertaintyAnalysisSettings.ReSampleRPFs = true;
                project.EffectSettings.ImputeMissingHazardDoses = true;
                project.ConcentrationModelSettings.UseComplexResidueDefinitions = true;
                project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
                project.ConcentrationModelSettings.ExtrapolateConcentrations = true;
                project.EffectModelSettings.UseAdjustmentFactors = true;
                var calculator = ActionCalculatorProvider.Create(definition.ActionType, project, false);

                var sources = calculator.GetRandomSources();
                foreach (var source in sources) {
                    Assert.IsTrue(
                        definition.UncertaintySources.Contains(source),
                        $"Uncertainty source {source} specified by action calculator is not found in definition of module {definition.ActionType}"
                    );
                }

                foreach (var source in definition.UncertaintySources) {
                    Assert.IsTrue(
                        sources.Contains(source),
                        $"Uncertainty source {source} specified in module definition is not used by action calculator of module {definition.ActionType}"
                    );
                }
            }
        }
    }
}
