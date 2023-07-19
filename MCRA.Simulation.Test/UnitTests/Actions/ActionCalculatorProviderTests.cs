using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the ActionCalculatorProvider
    /// </summary>
    [TestClass]
    public class ActionCalculatorProviderTests {
        /// <summary>
        /// Runs the ActionCalculatorProvider: Completeness
        /// </summary>
        [TestMethod]
        public void ActionCalculatorProvider_TestCompleteness() {
            var enumValues = Enum.GetValues(typeof(ActionType))
                .Cast<ActionType>()
                .Where(r => r != ActionType.Unknown);
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var calculator = ActionCalculatorProvider.Create(value, new ProjectDto(), false);
                Assert.IsNotNull(calculator);
            }
        }
        /// <summary>
        /// Runs the ActionCalculatorProvider: CalculatorActionTypes
        /// </summary>
        [TestMethod]
        public void ActionCalculatorProvider_TestCalculatorActionTypes() {
            var enumValues = Enum.GetValues(typeof(ActionType))
                .Cast<ActionType>()
                .Where(r => r != ActionType.Unknown);
            // Check whether the calculator action type corresponds with the requested action type.
            foreach (var value in enumValues) {
                var calculator = ActionCalculatorProvider.Create(value, new ProjectDto(), false);
                Assert.AreEqual(value, calculator.ActionType);
            }
        }

        /// <summary>
        /// Runs the ActionCalculatorProvider: SettingsSummarizers
        /// </summary>
        [TestMethod]
        public void ActionCalculatorProvider_TestSettingsSummarizers() {
            var enumValues = Enum.GetValues(typeof(ActionType))
                .Cast<ActionType>()
                .Where(r => r != ActionType.Unknown);
            foreach (var value in enumValues) {
                var calculator = ActionCalculatorProvider.Create(value, new ProjectDto(), true);
                var settingsSummary = calculator.SummarizeSettings();

                var selectionSettings = calculator.ModuleDefinition.SelectionSettingsItems.ToHashSet();
                var calculationSettings = calculator.ModuleDefinition.CalculationSettingsItems.ToHashSet();
                var outputSettings = calculator.ModuleDefinition.OutputSettingsItems.ToHashSet();

                var settingsSummaryRecords = settingsSummary.SummaryRecords
                    .Where(r => r is ActionSettingSummaryRecord)
                    .Cast<ActionSettingSummaryRecord>();
                foreach (var item in settingsSummaryRecords) {
                    var isSelectionSetting = selectionSettings.Contains(item.SettingsItemType);
                    var isCalculationSetting = calculationSettings.Contains(item.SettingsItemType);
                    var isOutputSetting = outputSettings.Contains(item.SettingsItemType);
                    Assert.IsTrue(
                        item.SettingsItemType == SettingsItemType.Undefined || isSelectionSetting || isCalculationSetting || isOutputSetting,
                        $"The {value} action calculator is missing a module definition setting for '{item.SettingsItemType}'"
                    );
                }
            }
        }
    }
}
