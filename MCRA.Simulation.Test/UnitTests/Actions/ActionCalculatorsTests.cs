using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
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
                    var tiers = definition.TemplateSettings;
                    if (tiers?.Any() ?? false) {
                        foreach (var kvp in tiers) {
                            ActionSettingsManagerBase.SetTier(project, kvp.Key);
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
                    var tiers = definition.TemplateSettings?.Values;
                    if (tiers?.Any() ?? false) {
                        var tierEnumDisplayNames = settingsManager.GetAvailableTiers();
                        foreach (var tier in tiers) {
                            Assert.AreEqual(tierEnumDisplayNames[tier.Tier], tier.Name);
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
                project.ProcessingFactorsSettings.ResampleProcessingFactors = true;
                project.OccurrencePatternsSettings.RecomputeOccurrencePatterns = true;
                project.ConcentrationModelsSettings.ResampleConcentrations = true;
                project.ConsumptionsSettings.ResampleIndividuals = true;
                project.HumanMonitoringDataSettings.ResampleHbmIndividuals = true;
                project.ConsumptionsSettings.ResamplePortions = true;
                project.DietaryExposuresSettings.ResampleImputationExposureDistributions = true;
                project.NonDietaryExposuresSettings.ResampleNonDietaryExposures = true;
                project.InterSpeciesConversionsSettings.ResampleInterspecies = true;
                project.IntraSpeciesFactorsSettings.ResampleIntraSpecies = true;
                project.ActiveSubstancesSettings.ResampleAssessmentGroupMemberships = true;
                project.KineticModelsSettings.ResampleKineticModelParameters = true;
                project.PbkModelsSettings.ResamplePbkModelParameters = true;
                project.HazardCharacterisationsSettings.ResampleRPFs = true;
                project.HazardCharacterisationsSettings.ImputeMissingHazardDoses = true;
                project.ConcentrationsSettings.UseComplexResidueDefinitions = true;
                project.ConcentrationsSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
                project.ConcentrationsSettings.ExtrapolateConcentrations = true;
                project.SingleValueRisksSettings.UseAdjustmentFactors = true;
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
