using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.General.ModuleDefinitions.Settings;
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
                            settingsManager.SetTier(project, kvp.Key, true);
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
                project.GetModuleConfiguration<ProcessingFactorsModuleConfig>().ReSampleProcessingFactors = true;
                project.GetModuleConfiguration<OccurrencePatternsModuleConfig>().RecomputeOccurrencePatterns = true;
                project.GetModuleConfiguration<ConcentrationModelsModuleConfig>().ReSampleConcentrations = true;
                project.GetModuleConfiguration<ConsumptionsModuleConfig>().ResampleIndividuals = true;
                project.GetModuleConfiguration<HumanMonitoringDataModuleConfig>().ResampleHBMIndividuals = true;
                project.GetModuleConfiguration<ConsumptionsModuleConfig>().ReSamplePortions = true;
                project.GetModuleConfiguration<DietaryExposuresModuleConfig>().ReSampleImputationExposureDistributions = true;
                project.GetModuleConfiguration<NonDietaryExposuresModuleConfig>().ReSampleNonDietaryExposures = true;
                project.GetModuleConfiguration<InterSpeciesConversionsModuleConfig>().ReSampleInterspecies = true;
                project.GetModuleConfiguration<IntraSpeciesFactorsModuleConfig>().ReSampleIntraSpecies = true;
                project.GetModuleConfiguration<ActiveSubstancesModuleConfig>().ReSampleAssessmentGroupMemberships = true;
                project.GetModuleConfiguration<KineticModelsModuleConfig>().ResampleKineticModelParameters = true;
                project.GetModuleConfiguration<HazardCharacterisationsModuleConfig>().ReSampleRPFs = true;
                project.GetModuleConfiguration<HazardCharacterisationsModuleConfig>().ImputeMissingHazardDoses = true;
                project.GetModuleConfiguration<ConcentrationsModuleConfig>().UseComplexResidueDefinitions = true;
                project.GetModuleConfiguration<ConcentrationsModuleConfig>().FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
                project.GetModuleConfiguration<ConcentrationsModuleConfig>().ExtrapolateConcentrations = true;
                project.GetModuleConfiguration<SingleValueRisksModuleConfig>().UseAdjustmentFactors = true;
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
