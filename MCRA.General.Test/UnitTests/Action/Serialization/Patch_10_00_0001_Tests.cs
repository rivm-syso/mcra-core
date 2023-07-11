using System.Xml;
using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_00_0001_Tests : ProjectSettingsSerializerTestsBase {
        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix1() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>Blood</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(9, 2, 8));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Blood,
                settingsDto.HumanMonitoringSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix2() {
            var settingsXml =
                "<KineticModelSettings>" +
                "  <CodeCompartment>Blood</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(9, 2, 8));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.WholeBody,
                settingsDto.HumanMonitoringSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix3() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>CLiver</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(9, 2, 8));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Liver,
                settingsDto.HumanMonitoringSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestHbmTargetMatrix4() {
            var settingsXml =
                "<HumanMonitoringSettings></HumanMonitoringSettings>" +
                "<KineticModelSettings>" +
                "  <CodeCompartment>Aaaaaaaaaa</CodeCompartment>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(9, 2, 8));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settingsDto);
            Assert.AreEqual(
                BiologicalMatrix.Undefined,
                settingsDto.HumanMonitoringSettings.TargetMatrix
            );
        }

        /// <summary>
        /// Test patch 10.00.0001.
        /// Remove KineticModelSettings/NumberOfIndividuals
        /// </summary>
        [TestMethod]
        public void Patch_10_00_0001_TestRemoveKineticModelsNumberOfIndividuals() {
            var settingsXml =
                "<KineticModelSettings>" +
                "  <NumberOfIndividuals>100</NumberOfIndividuals>" +
                "</KineticModelSettings>";
            var xml = createMockSettingsXml(settingsXml, new Version(10, 0, 0));
            var node = getXmlNode(xml, "//Project//KineticModelSettings//NumberOfIndividuals");
            Assert.IsNotNull(node);

            var patchedXml = applyPatch(xml, "Patch-10.00.0001.xslt");

            static XmlNode getXmlNode(string patchedXml, string path) {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(patchedXml);
                var root = xmlDoc.DocumentElement;
                var result = root.SelectSingleNode(path);
                return result;
            }

            node = getXmlNode(patchedXml, "/Project/KineticModelSettings/NumberOfIndividuals");
            Assert.IsNull(node);
        }
    }
}
