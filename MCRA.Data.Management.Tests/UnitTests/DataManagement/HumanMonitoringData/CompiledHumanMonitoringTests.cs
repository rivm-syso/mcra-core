using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledHumanMonitoringTests : CompiledTestsBase {
        protected Func<IDictionary<string, HumanMonitoringSurvey>> _getSurveysDelegate;
        protected Func<IDictionary<string, Individual>> _getIndividualsDelegate;
        protected Func<IDictionary<string, HumanMonitoringSample>> _getSamplesDelegate;
        protected Func<IDictionary<string, AnalyticalMethod>> _getAnalyticalMethodsDelegate;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            //explicitly set data sources
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Concentrations);
        }

        [TestMethod]
        public void CompiledHumanMonitoringData_TestIndividualsOnly() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals")
            );


            var individuals = _getIndividualsDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();

            Assert.AreEqual(3, surveys.Count);
            Assert.AreEqual(5, individuals.Count);

            CollectionAssert.AreEquivalent(new[] { "s1", "s2", "s3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
        }

        [TestMethod]
        public void CompiledHumanMonitoringData_TestIndividualsOnlyWithSurveyScope() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var individuals = _getIndividualsDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();

            Assert.AreEqual(1, surveys.Count);
            Assert.AreEqual(2, individuals.Count);

            CollectionAssert.AreEquivalent(new[] { "s2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
        }

        [TestMethod]
        public void CompiledHumanMonitoringData_TestDataIndividualsOnlyWithSurveyData() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringSurveys, @"HumanMonitoringDataTests/HumanMonitoringSurveys")
            );

            var individuals = _getIndividualsDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "S1", "S2", "s3" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "1", "2", "3", "4", "5" }, individuals.Keys.ToList());
        }

        [TestMethod]
        public void CompiledHumanMonitoringData_TestSamplesIndividuals() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple")
            );

            var individuals = _getIndividualsDelegate.Invoke();
            var samples = _getSamplesDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();

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
        public void CompiledHumanMonitoringData_TestSamplesIndividualsSurveyFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var surveys = _getSurveysDelegate.Invoke();
            var individuals = _getIndividualsDelegate.Invoke();
            var samples = _getSamplesDelegate.Invoke();

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
        public void CompiledHumanMonitoringData_TestSampleAnalysesIndividuals() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple")
            );

            var individuals = _getIndividualsDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();
            var samples = _getSamplesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();

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
            Assert.AreEqual(0, samples["HS3"].SampleAnalyses.Count);
            Assert.AreEqual("AS4", samples["HS4"].SampleAnalyses.Select(s => s.Code).Single());
        }

        [TestMethod]
        public void CompiledHumanMonitoringData_TestSampleAnalysesIndividualsSurveyFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var individuals = _getIndividualsDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();
            var samples = _getSamplesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "s2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS3", "HS4" }, samples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());

            Assert.AreEqual(1, samples["HS3"].Individual.Id);
            Assert.AreEqual(2, samples["HS4"].Individual.Id);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreNotEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));

            Assert.AreEqual(0, samples["HS3"].SampleAnalyses.Count);
            Assert.AreEqual("AS4", samples["HS4"].SampleAnalyses.Select(s => s.Code).Single());
        }


        [TestMethod]
        public void CompiledHumanMonitoringData_TestSampleAnalysesConcentrationsIndividuals() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple"),
                (ScopingType.HumanMonitoringSampleConcentrations, @"HumanMonitoringDataTests/HumanMonitoringSampleConcentrations")
            );

            var individuals = _getIndividualsDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();
            var samples = _getSamplesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();

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

            Assert.AreEqual(0, samples["HS3"].SampleAnalyses.Count);

            var s4 = samples["HS4"].SampleAnalyses.Single();
            Assert.AreEqual("AS4", s4.Code);
            CollectionAssert.AreEquivalent(new[] { "Q", "S", "T" }, s4.Concentrations.Keys.Select(c => c.Code).ToList());
        }


        [TestMethod]
        public void CompiledHumanMonitoringData_TestSampleAnalysesConcentrationsIndividualsFilterSurveyCompounds() {
            _rawDataProvider.SetDataTables(
                (ScopingType.HumanMonitoringIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.HumanMonitoringAnalyticalMethods, @"HumanMonitoringDataTests/AnalyticalMethodsSimple"),
                (ScopingType.HumanMonitoringAnalyticalMethodCompounds, @"HumanMonitoringDataTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.HumanMonitoringSamples, @"HumanMonitoringDataTests/HumanMonitoringSamplesSimple"),
                (ScopingType.HumanMonitoringSampleAnalyses, @"HumanMonitoringDataTests/HumanMonitoringSampleAnalysesSimple"),
                (ScopingType.HumanMonitoringSampleConcentrations, @"HumanMonitoringDataTests/HumanMonitoringSampleConcentrations")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);
            _rawDataProvider.SetFilterCodes(ScopingType.HumanMonitoringSurveys, ["s2"]);

            var individuals = _getIndividualsDelegate.Invoke();
            var surveys = _getSurveysDelegate.Invoke();
            var samples = _getSamplesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "s2" }, surveys.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "3", "4" }, individuals.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "HS3", "HS4" }, samples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());

            Assert.AreEqual(1, samples["HS3"].Individual.Id);
            Assert.AreEqual(2, samples["HS4"].Individual.Id);
            Assert.AreEqual("3", samples["HS3"].Individual.Code);
            Assert.AreEqual("4", samples["HS4"].Individual.Code);

            Assert.AreNotEqual(0, samples.Values.Sum(s => s.SampleAnalyses.Count));

            Assert.AreEqual(0, samples["HS3"].SampleAnalyses.Count);

            var s4 = samples["HS4"].SampleAnalyses.Single();
            Assert.AreEqual("AS4", s4.Code);
            CollectionAssert.AreEquivalent(new[] { "S" }, s4.Concentrations.Keys.Select(c => c.Code).ToList());
        }
    }
}
