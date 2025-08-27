using MCRA.General.Action.Serialization;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0012_00_Tests : ProjectSettingsSerializerTestsBase {

        /// <summary>
        /// If in previous actions the hbm option convert to single target was selected, then in the new version
        /// the option apply kinetic conversions should be automatically selected; otherwise nothing should be transformed.
        /// </summary>
        [TestMethod]
        [DataRow(true, true)]
        [DataRow(false, false)]
        public void Patch_10_00_0012_ConvertToSingleTarget_TestKineticConversions(
            bool oldConvertToSingleTargetMatrix,
            bool newApplyKineticConversions
            ) {
            var settingsXml =
                "<HumanMonitoringSettings>" +
                $"  <HbmConvertToSingleTargetMatrix>{oldConvertToSingleTargetMatrix.ToString().ToLower()}</HbmConvertToSingleTargetMatrix>" +
                "</HumanMonitoringSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 10));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                newApplyKineticConversions,
                settingsDto.HumanMonitoringAnalysisSettings.ApplyKineticConversions
            );
        }
    }
}
