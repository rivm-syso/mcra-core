using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.OccurrencePatterns;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.OccurrencePatterns {
    [TestClass]
    public class OccurrencePatternsSettingsSummarizerTest {

        /// <summary>
        /// Validate correct visibility of ScaleUpOccurencePatterns.
        /// </summary>
        [TestMethod]
        [DataRow(false, false)]
        [DataRow(true, true)]
        public void Summarize_UseAgriculturalUsePercentage_ShouldShowHideScaleUpOccurrencePatterns(
            bool useAgriculturalUsePercentage,
            bool showScaleUpOccurrencePatterns
        ) {
            // Arrange
            var config = new OccurrencePatternsModuleConfig {
                UseAgriculturalUsePercentage = useAgriculturalUsePercentage
            };
            var summarizer = new OccurrencePatternsSettingsSummarizer(config);

            // Act
            var section = summarizer.Summarize(true);

            // Assert
            Assert.AreEqual(section.SummaryRecords.Exists(r => (r as ActionSettingSummaryRecord).SettingsItemType == SettingsItemType.ScaleUpOccurencePatterns), showScaleUpOccurrencePatterns);
        }

        /// <summary>
        /// Validate correct visibility of RestrictOccurencePatternScalingToAuthorisedUses.
        /// </summary>
        [TestMethod]
        [DataRow(false, false, false)]
        [DataRow(false, true, false)]
        [DataRow(true, false, false)]
        [DataRow(true, true, true)]
        public void Summarize_UseAgriculturalUsePercentageAndScaleUpOccurrencePatterns_ShouldShowHideRestrictToAuthorisedUses(
            bool useAgriculturalUsePercentage,
            bool scaleUpOccurrencePatterns,
            bool showRestrictScalingToAuthorisedUses
        ) {
            // Arrange
            var config = new OccurrencePatternsModuleConfig {
                //ActionType = ActionType.Risks,
                UseAgriculturalUsePercentage = useAgriculturalUsePercentage,
                ScaleUpOccurencePatterns = scaleUpOccurrencePatterns,
            };
            var summarizer = new OccurrencePatternsSettingsSummarizer(config);

            // Act
            var section = summarizer.Summarize(true);

            // Assert
            Assert.AreEqual(section.SummaryRecords.Exists(r => (r as ActionSettingSummaryRecord).SettingsItemType == SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses), showRestrictScalingToAuthorisedUses);
        }
    }
}
