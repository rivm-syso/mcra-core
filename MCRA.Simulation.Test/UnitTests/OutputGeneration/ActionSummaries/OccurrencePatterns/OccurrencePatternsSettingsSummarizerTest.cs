using MCRA.General;
using MCRA.General.Action.Settings;
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
        public void Summarize_UseAgriculturalUsePercentage_ShouldShowHideScaleUpOccurrencePatterns(bool useAgriculturalUsePercentage, bool showScaleUpOccurrencePatterns) {
            // Arrange
            var summarizer = new OccurrencePatternsSettingsSummarizer();
            var project = new ProjectDto {
                ActionType = ActionType.OccurrencePatterns,
                AgriculturalUseSettings = new AgriculturalUseSettings { UseAgriculturalUsePercentage = useAgriculturalUsePercentage },
                CalculationActionTypes = new HashSet<ActionType> { ActionType.OccurrencePatterns }
            };

            // Act
            var section = summarizer.Summarize(project);

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
        public void Summarize_UseAgriculturalUsePercentageAndScaleUpOccurrencePatterns_ShouldShowHideRestrictToAuthorisedUses(bool useAgriculturalUsePercentage,
                                                                                                                              bool scaleUpOccurrencePatterns,
                                                                                                                              bool showRestrictScalingToAuthorisedUses) {
            // Arrange
            var summarizer = new OccurrencePatternsSettingsSummarizer();
            var project = new ProjectDto {
                //ActionType = ActionType.Risks,
                CalculationActionTypes = new HashSet<ActionType> { ActionType.OccurrencePatterns },
                AgriculturalUseSettings = new AgriculturalUseSettings { UseAgriculturalUsePercentage = useAgriculturalUsePercentage,
                                                                           ScaleUpOccurencePatterns = scaleUpOccurrencePatterns,
                                                                           UseAgriculturalUseTable = true }
            };

            // Act
            var section = summarizer.Summarize(project);

            // Assert
            Assert.AreEqual(section.SummaryRecords.Exists(r => (r as ActionSettingSummaryRecord).SettingsItemType == SettingsItemType.RestrictOccurencePatternScalingToAuthorisedUses), showRestrictScalingToAuthorisedUses);
        }
    }
}
