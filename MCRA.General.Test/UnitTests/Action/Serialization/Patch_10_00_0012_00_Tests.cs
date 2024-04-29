using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                settingsDto.HumanMonitoringSettings.ApplyKineticConversions
            );
        }

        /// <summary>
        /// If in previous actions the code compartment setting was specified, then we expect the
        /// new compartment codes lists to contain one element with this code.
        /// </summary>
        [TestMethod]
        [DataRow("CLiver")]
        public void CodeCompartment_ShouldBeInCompartmentCodesList(
            string codeCompartment
        ) {
            var settingsXml =
                "<KineticModelSettings>" +
                $"  <CodeCompartment>{codeCompartment}</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 10));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                codeCompartment,
                settingsDto.KineticModelSettings.CompartmentCodes.Single()
            );
        }

        /// <summary>
        /// If in previous actions the code compartment setting was not specified, then we expect the
        /// new compartment codes lists to be empty.
        /// </summary>
        [TestMethod]
        public void CodeCompartment_NotSpecifiedShouldYieldEmptyList() {
            var settingsXml =
                "<KineticModelSettings>" +
                $"  <CodeCompartment></CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 10));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsFalse(settingsDto.KineticModelSettings.CompartmentCodes.Any());
        }

        /// <summary>
        /// If in previous actions the kinetic model settings were not specified at all, then 
        /// we expect the new compartment codes lists to be null.
        /// </summary>
        [TestMethod]
        public void CodeCompartment_NoKineticModelSettingsShouldYieldNoList() {
            var settingsXml = "";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 10));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.IsFalse(settingsDto.KineticModelSettings.CompartmentCodes.Any());
        }
    }
}
