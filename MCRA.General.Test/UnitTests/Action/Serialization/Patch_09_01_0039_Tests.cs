using MCRA.General.Action.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_09_01_0039_Tests : ProjectSettingsSerializerTestsBase {
        [TestMethod]
        public void Patch_09_01_0039_TestMatchIndividualSubsetWithPopulation() {
            Func<bool, bool, bool, string> settingsXml = (x1, x2, x3) =>
                "<SubsetSettings>" +
                $"<PopulationSubsetSelection>true</PopulationSubsetSelection>" +
                $"<MatchIndividualsWithPopulation>{x1.ToString().ToLower()}</MatchIndividualsWithPopulation>" +
                $"<MatchIndividualDaysWithPopulation>{x2.ToString().ToLower()}</MatchIndividualDaysWithPopulation> " +
                $"<IndividualDaySubsetSelection>{x3.ToString().ToLower()}</IndividualDaySubsetSelection>" +
                "</SubsetSettings>";

            var xml = createMockSettingsXml(settingsXml(true, false, false), version: new Version(9, 1, 37));
            var settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            var config = settingsDto.ConsumptionsSettings;
            Assert.AreEqual(IndividualSubsetType.MatchToPopulationDefinition, config.MatchIndividualSubsetWithPopulation);
            Assert.IsTrue(config.PopulationSubsetSelection);

            xml = createMockSettingsXml(settingsXml(false, true, false), version: new Version(9, 1, 37));
            settingsDto = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            config = settingsDto.ConsumptionsSettings;
            Assert.AreEqual(IndividualSubsetType.IgnorePopulationDefinition, config.MatchIndividualSubsetWithPopulation);
            Assert.IsTrue(config.PopulationSubsetSelection);
        }
    }
}
