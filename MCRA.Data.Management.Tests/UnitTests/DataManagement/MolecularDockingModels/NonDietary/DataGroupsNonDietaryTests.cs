using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataGroupsNonDietaryTests : CompiledTestsBase {

        /// <summary>
        /// Check the nominal non-dietary exposures of non-dietary survey 1 with unmatched individual exposures.
        /// </summary>
        [TestMethod]
        public void NonDietaryDataTests1() {
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.NonDietary, SourceTableGroup.Compounds, SourceTableGroup.Survey]);

            var nonDietaryExposureSets = _compiledDataManager.GetAllNonDietaryExposureSets();

            var nonDietarySurvey1 = nonDietaryExposureSets.Select(c => c.NonDietarySurvey).Distinct().Single(nds => nds.Code == "NonDietarySurvey1");
            var exposuresIndividualsSurvey1 = nonDietaryExposureSets.Where(c => c.NonDietarySurvey == nonDietarySurvey1).ToList();
            //var exposuresIndividualsSurvey1 = nonDietarySurvey1.NonDietaryExposureIndividualSets;

            var compoundA = _compiledDataManager.GetAllCompounds()["CompoundA"];
            var compoundD = _compiledDataManager.GetAllCompounds()["CompoundD"];

            Assert.AreEqual(3, exposuresIndividualsSurvey1.Count);

            var nonDietaryExposuresNd1 = exposuresIndividualsSurvey1.Single(nde => nde.IndividualCode == "ND1");
            var nonminalNonDietaryExposuresNd1 = nonDietaryExposuresNd1.NonDietaryExposures;
            Assert.AreEqual(2, nonminalNonDietaryExposuresNd1.Count);
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd1.Single(nnde => nnde.Compound == compoundA).Dermal);
            Assert.AreEqual(2, nonminalNonDietaryExposuresNd1.Single(nnde => nnde.Compound == compoundA).Oral);
            Assert.AreEqual(3, nonminalNonDietaryExposuresNd1.Single(nnde => nnde.Compound == compoundA).Inhalation);
            Assert.AreEqual(2, nonminalNonDietaryExposuresNd1.Single(nnde => nnde.Compound == compoundD).Dermal);
            Assert.AreEqual(3, nonminalNonDietaryExposuresNd1.Single(nnde => nnde.Compound == compoundD).Oral);
            Assert.AreEqual(4, nonminalNonDietaryExposuresNd1.Single(nnde => nnde.Compound == compoundD).Inhalation);

            var nonDietaryExposuresNd2 = exposuresIndividualsSurvey1.Single(nde => nde.IndividualCode == "ND2");
            var nonminalNonDietaryExposuresNd2 = nonDietaryExposuresNd2.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd2.Count);
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd2.Single(nnde => nnde.Compound == compoundA).Dermal);
            Assert.AreEqual(2, nonminalNonDietaryExposuresNd2.Single(nnde => nnde.Compound == compoundA).Oral);
            Assert.AreEqual(3, nonminalNonDietaryExposuresNd2.Single(nnde => nnde.Compound == compoundA).Inhalation);

            var nonDietaryExposuresNd3 = exposuresIndividualsSurvey1.Single(nde => nde.IndividualCode == "ND3");
            var nonminalNonDietaryExposuresNd3 = nonDietaryExposuresNd3.NonDietaryExposures;
            Assert.AreEqual(2, nonminalNonDietaryExposuresNd3.Count);
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd3.Single(nnde => nnde.Compound == compoundA).Dermal);
            Assert.AreEqual(2, nonminalNonDietaryExposuresNd3.Single(nnde => nnde.Compound == compoundA).Oral);
            Assert.AreEqual(3, nonminalNonDietaryExposuresNd3.Single(nnde => nnde.Compound == compoundA).Inhalation);
            Assert.AreEqual(2, nonminalNonDietaryExposuresNd3.Single(nnde => nnde.Compound == compoundD).Dermal);
            Assert.AreEqual(3, nonminalNonDietaryExposuresNd3.Single(nnde => nnde.Compound == compoundD).Oral);
            Assert.AreEqual(4, nonminalNonDietaryExposuresNd3.Single(nnde => nnde.Compound == compoundD).Inhalation);
        }

        /// <summary>
        /// Check the nominal non-dietary exposures of non-dietary survey 2 with matched individual exposures.
        /// </summary>
        [TestMethod]
        public void NonDietaryDataTests2() {
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.NonDietary, SourceTableGroup.Compounds, SourceTableGroup.Survey]);

            var nonDietaryExposureSets = _compiledDataManager.GetAllNonDietaryExposureSets();

            var nonDietarySurvey2 = nonDietaryExposureSets.Select(c => c.NonDietarySurvey).Distinct().Single(nds => nds.Code == "NonDietarySurvey2");
            var exposuresMatchedIndividualsSurvey1 = nonDietaryExposureSets.Where(c => c.NonDietarySurvey == nonDietarySurvey2).ToList();

            var compoundA = _compiledDataManager.GetAllCompounds()["CompoundA"];
            var individuals = _compiledDataManager.GetAllIndividuals();

            Assert.AreEqual(7, exposuresMatchedIndividualsSurvey1.Count);

            var nonDietaryExposuresIndividual1 = exposuresMatchedIndividualsSurvey1.Single(nde => nde.IndividualCode == "1");
            var nonminalNonDietaryExposuresNd1 = nonDietaryExposuresIndividual1.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd1.Count);
            Assert.AreEqual(4.1, nonminalNonDietaryExposuresNd1.Single(nnde => nnde.Compound == compoundA).Dermal);

            var nonDietaryExposuresIndividual2 = exposuresMatchedIndividualsSurvey1.Single(nde => nde.IndividualCode == "2");
            var nonminalNonDietaryExposuresNd2 = nonDietaryExposuresIndividual2.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd2.Count);
            Assert.AreEqual(4.2, nonminalNonDietaryExposuresNd2.Single(nnde => nnde.Compound == compoundA).Dermal);

            var nonDietaryExposuresIndividual3 = exposuresMatchedIndividualsSurvey1.Single(nde => nde.IndividualCode == "3");
            var nonminalNonDietaryExposuresNd3 = nonDietaryExposuresIndividual3.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd3.Count);
            Assert.AreEqual(4.3, nonminalNonDietaryExposuresNd3.Single(nnde => nnde.Compound == compoundA).Dermal);

            var nonDietaryExposuresIndividual4 = exposuresMatchedIndividualsSurvey1.Single(nde => nde.IndividualCode == "4");
            var nonminalNonDietaryExposuresNd4 = nonDietaryExposuresIndividual4.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd4.Count);
            Assert.AreEqual(4.4, nonminalNonDietaryExposuresNd4.Single(nnde => nnde.Compound == compoundA).Dermal);

            var nonDietaryExposuresIndividual5 = exposuresMatchedIndividualsSurvey1.Single(nde => nde.IndividualCode == "5");
            var nonminalNonDietaryExposuresNd5 = nonDietaryExposuresIndividual5.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd5.Count);
            Assert.AreEqual(4.5, nonminalNonDietaryExposuresNd5.Single(nnde => nnde.Compound == compoundA).Dermal);

            var nonDietaryExposuresIndividual6 = exposuresMatchedIndividualsSurvey1.Single(nde => nde.IndividualCode == "6");
            var nonminalNonDietaryExposuresNd6 = nonDietaryExposuresIndividual6.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd6.Count);
            Assert.AreEqual(4.6, nonminalNonDietaryExposuresNd6.Single(nnde => nnde.Compound == compoundA).Dermal);

            var nonDietaryExposuresIndividual7 = exposuresMatchedIndividualsSurvey1.Single(nde => nde.IndividualCode == "7");
            var nonminalNonDietaryExposuresNd7 = nonDietaryExposuresIndividual7.NonDietaryExposures;
            Assert.AreEqual(1, nonminalNonDietaryExposuresNd7.Count);
            Assert.AreEqual(4.7, nonminalNonDietaryExposuresNd7.Single(nnde => nnde.Compound == compoundA).Dermal);
        }
    }
}
