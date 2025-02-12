using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledNonDietaryTests : CompiledTestsBase {

        protected Func<ICollection<NonDietaryExposureSet>> _getItemsDelegate;

        [TestMethod]
        public void CompiledNonDietary_TestSurveysOnly() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys")
            );

            var ndSets = _getItemsDelegate.Invoke();
            Assert.AreEqual(0, ndSets.Count);
        }

        [TestMethod]
        public void CompiledNonDietary_TestSurveysOnlyScope() {
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, ["s2"]);
            var ndSets = _getItemsDelegate.Invoke();

            Assert.AreEqual(0, ndSets.Count);
        }

        [TestMethod]
        public void CompiledNonDietary_TestSurveysOnlyWithSurveyScope() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, ["s2"]);
            var ndSets = _getItemsDelegate.Invoke();

            Assert.AreEqual(0, ndSets.Count);
        }

        [TestMethod]
        public void CompiledNonDietary_TestSurveysAndExposures() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );

            var ndSets = _getItemsDelegate.Invoke();

            Assert.AreEqual(7, ndSets.Count);
            Assert.IsTrue(ndSets.All(s => string.IsNullOrWhiteSpace(s.Code)));
            Assert.AreEqual(
                "S1-1-A;S1-1-B;S1-2-C;S1-2-D;S1-3-A;S1-3-D;S1-4-B;S1-4-D;S2-4-C;S2-4-E;S2-5-A;S2-5-F;S2-6-C;S2-6-F",
                string.Join(";", ndSets
                .SelectMany(s => s.NonDietaryExposures,
                           (s, e) => $"{s.NonDietarySurvey.Code}-{e.IdIndividual}-{e.Compound.Code}"))
            );
        }

        [TestMethod]
        public void CompiledNonDietary_TestSurveysAndExposuresFilterCompoundsTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var ndSets = _getItemsDelegate.Invoke();

            Assert.AreEqual(5, ndSets.Count);
            Assert.IsTrue(ndSets.All(s => string.IsNullOrWhiteSpace(s.Code)));
            Assert.AreEqual(
                "S1-1-B;S1-2-C;S1-4-B;S2-4-C;S2-6-C",
                string.Join(";", ndSets
                .SelectMany(s => s.NonDietaryExposures,
                           (s, e) => $"{s.NonDietarySurvey.Code}-{e.IdIndividual}-{e.Compound.Code}"))
            );
        }

        [TestMethod]
        public void CompiledNonDietary_TestFilterSurvey() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, ["s2"]);

            var ndSets = _getItemsDelegate.Invoke();

            Assert.AreEqual(3, ndSets.Count);
            Assert.IsTrue(ndSets.All(s => string.IsNullOrWhiteSpace(s.Code)));
            Assert.AreEqual(
                "S2-4-C;S2-4-E;S2-5-A;S2-5-F;S2-6-C;S2-6-F",
                string.Join(";", ndSets
                .SelectMany(s => s.NonDietaryExposures,
                           (s, e) => $"{s.NonDietarySurvey.Code}-{e.IdIndividual}-{e.Compound.Code}"))
            );
        }

        [TestMethod]
        public void CompiledNonDietary_TestCombineWithIndividualsTest() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Survey);
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );

            var ndSets = _getItemsDelegate.Invoke();

            Assert.AreEqual(7, ndSets.Count);
            Assert.IsTrue(ndSets.All(s => string.IsNullOrWhiteSpace(s.Code)));
            Assert.AreEqual(
                "S1-1-A;S1-1-B;S1-2-C;S1-2-D;S1-3-A;S1-3-D;S1-4-B;S1-4-D;S2-4-C;S2-4-E;S2-5-A;S2-5-F;S2-6-C;S2-6-F",
                string.Join(";", ndSets
                .SelectMany(s => s.NonDietaryExposures,
                           (s, e) => $"{s.NonDietarySurvey.Code}-{e.IdIndividual}-{e.Compound.Code}"))
            );
        }
    }
}
