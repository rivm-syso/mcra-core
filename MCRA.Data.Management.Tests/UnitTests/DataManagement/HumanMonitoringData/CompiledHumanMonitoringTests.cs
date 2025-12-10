using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledHumanMonitoringTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestIndividualsOnly(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals")
            );

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);

            Assert.HasCount(3, surveys);
            Assert.HasCount(5, individuals);

            CollectionAssert.AreEquivalent(new[] { "s1", "s2", "s3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestIndividualsOnlyWithSurveyScope(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals")
            );
            RawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);

            Assert.HasCount(1, surveys);
            Assert.HasCount(2, individuals);

            CollectionAssert.AreEquivalent(new[] { "s2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestDataIndividualsOnlyWithSurveyData(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringSurveys, @"HumanMonitoringDataTests/HumanMonitoringSurveys")
            );

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);

            CollectionAssert.AreEquivalent(new[] { "S1", "S2", "s3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestSamplesIndividuals(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple")
            );

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var samples = GetAllHumanMonitoringSamples(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);

            CollectionAssert.AreEquivalent(new[] { "s1", "s2", "s3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS1", "HS2", "HS3", "HS4" }, samples.Keys.ToList());
            Assert.AreEqual(1, samples["HS1"].Individual.Id);
            Assert.AreEqual(2, samples["HS2"].Individual.Id);
            Assert.AreEqual(3, samples["HS3"].Individual.Id);
            Assert.AreEqual(4, samples["HS4"].Individual.Id);
            Assert.AreEqual("1", samples["HS1"].Individual.Code);
            Assert.AreEqual("2", samples["HS2"].Individual.Code);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestSamplesIndividualsSurveyFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var surveys = GetAllHumanMonitoringSurveys(managerType);
            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var samples = GetAllHumanMonitoringSamples(managerType);

            CollectionAssert.AreEquivalent(new[] { "s2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS3", "HS4" }, samples.Keys.ToList());
            Assert.AreEqual(1, samples["HS3"].Individual.Id);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual(2, samples["HS4"].Individual.Id);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestSampleAnalysesIndividuals(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple")
            );

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);
            var samples = GetAllHumanMonitoringSamples(managerType);
            var analyticalMethods = GetAllHumanMonitoringAnalyticalMethods(managerType);

            CollectionAssert.AreEquivalent(new[] { "s1", "s2", "s3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS1", "HS2", "HS3", "HS4" }, samples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());

            Assert.AreEqual(1, samples["HS1"].Individual.Id);
            Assert.AreEqual(2, samples["HS2"].Individual.Id);
            Assert.AreEqual(3, samples["HS3"].Individual.Id);
            Assert.AreEqual(4, samples["HS4"].Individual.Id);
            Assert.AreEqual("1", samples["HS1"].Individual.Code);
            Assert.AreEqual("2", samples["HS2"].Individual.Code);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreNotEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));

            Assert.AreEqual("AS1", samples["HS1"].SampleAnalyses.Select(s => s.Code).Single());
            Assert.AreEqual("AS2", samples["HS2"].SampleAnalyses.Select(s => s.Code).Single());
            Assert.IsEmpty(samples["HS3"].SampleAnalyses);
            Assert.AreEqual("AS4", samples["HS4"].SampleAnalyses.Select(s => s.Code).Single());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestSampleAnalysesIndividualsSurveyFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);
            var samples = GetAllHumanMonitoringSamples(managerType);
            var analyticalMethods = GetAllHumanMonitoringAnalyticalMethods(managerType);

            CollectionAssert.AreEquivalent(new[] { "s2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS3", "HS4" }, samples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());

            Assert.AreEqual(1, samples["HS3"].Individual.Id);
            Assert.AreEqual(2, samples["HS4"].Individual.Id);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreNotEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));

            Assert.IsEmpty(samples["HS3"].SampleAnalyses);
            Assert.AreEqual("AS4", samples["HS4"].SampleAnalyses.Select(s => s.Code).Single());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestSampleAnalysesConcentrationsIndividuals(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple"),
                (ScopingType.HumanMonitoringSampleConcentrations, @"HumanMonitoringDataTests/HumanMonitoringSampleConcentrations")
            );

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);
            var samples = GetAllHumanMonitoringSamples(managerType);
            var analyticalMethods = GetAllHumanMonitoringAnalyticalMethods(managerType);

            CollectionAssert.AreEquivalent(new[] { "s1", "s2", "s3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS1", "HS2", "HS3", "HS4" }, samples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());

            Assert.AreEqual(1, samples["HS1"].Individual.Id);
            Assert.AreEqual(2, samples["HS2"].Individual.Id);
            Assert.AreEqual(3, samples["HS3"].Individual.Id);
            Assert.AreEqual(4, samples["HS4"].Individual.Id);
            Assert.AreEqual("1", samples["HS1"].Individual.Code);
            Assert.AreEqual("2", samples["HS2"].Individual.Code);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreNotEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));

            var s1 = samples["HS1"].SampleAnalyses.Single();
            Assert.AreEqual("AS1", s1.Code);
            CollectionAssert.AreEquivalent(new[] { "P", "Q" }, s1.Concentrations.Keys.Select(c => c.Code).ToList());

            var s2 = samples["HS2"].SampleAnalyses.Single();
            Assert.AreEqual("AS2", s2.Code);
            CollectionAssert.AreEquivalent(new[] { "R", "S" }, s2.Concentrations.Keys.Select(c => c.Code).ToList());

            Assert.IsEmpty(samples["HS3"].SampleAnalyses);

            var s4 = samples["HS4"].SampleAnalyses.Single();
            Assert.AreEqual("AS4", s4.Code);
            CollectionAssert.AreEquivalent(new[] { "Q", "S", "T" }, s4.Concentrations.Keys.Select(c => c.Code).ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledHumanMonitoringData_TestSampleAnalysesConcentrationsIndividualsFilterSurveyCompounds(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple"),
                (ScopingType.HumanMonitoringSampleConcentrations, @"HumanMonitoringDataTests/HumanMonitoringSampleConcentrations")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);
            RawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var individuals = GetAllHumanMonitoringIndividuals(managerType);
            var surveys = GetAllHumanMonitoringSurveys(managerType);
            var samples = GetAllHumanMonitoringSamples(managerType);
            var analyticalMethods = GetAllHumanMonitoringAnalyticalMethods(managerType);

            CollectionAssert.AreEquivalent(new[] { "s2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS3", "HS4" }, samples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());

            Assert.AreEqual(1, samples["HS3"].Individual.Id);
            Assert.AreEqual(2, samples["HS4"].Individual.Id);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreNotEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));

            Assert.IsEmpty(samples["HS3"].SampleAnalyses);

            var s4 = samples["HS4"].SampleAnalyses.Single();
            Assert.AreEqual("AS4", s4.Code);
            CollectionAssert.AreEquivalent(new[] { "S" }, s4.Concentrations.Keys.Select(c => c.Code).ToList());
        }
    }
}
