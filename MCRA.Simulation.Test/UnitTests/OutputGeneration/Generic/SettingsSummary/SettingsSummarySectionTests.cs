using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic {

    /// <summary>
    /// OutputGeneration, Generic, SettingsSummary
    /// </summary>
    [TestClass]
    public class SettingsSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test SettingsSummarySection view
        /// </summary>
        [TestMethod]
        public void SettingsSummarySection_Test1() {
            var section = new SettingsSummarySection();
            RawDataSourceVersionDto version = null;
            var tableGroup = new SourceTableGroup();
            var actionDataSummaryRecord = new ActionDataSummaryRecord() {
                SourceTableGroup = tableGroup,
                DataSourceName = version?.DataSourceName,
                DataSourcePath = version?.FullPath,
                Checksum = version?.Checksum,
                IdDataSourceVersion = version?.id ?? -1,
                Version = version?.VersionNumber ?? -1,
                VersionName = version?.Name,
                VersionDate = version?.UploadTimestamp,
                IsValid = version != null
            };
            section.SummarizeDataSource(actionDataSummaryRecord);
            Assert.AreEqual(1, section.DataSourceSummaryRecords.Count);
            AssertIsValidView(section);
        }
    }
}