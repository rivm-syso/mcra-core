using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledNonDietaryTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledNonDietary_TestSurveysOnly(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys")
            );

            var ndSets = GetAllNonDietaryExposureSets(managerType);
            Assert.IsEmpty(ndSets);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledNonDietary_TestSurveysOnlyScope(ManagerType managerType) {
            RawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, ["s2"]);
            var ndSets = GetAllNonDietaryExposureSets(managerType);

            Assert.IsEmpty(ndSets);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledNonDietary_TestSurveysOnlyWithSurveyScope(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys")
            );
            RawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, ["s2"]);
            var ndSets = GetAllNonDietaryExposureSets(managerType);

            Assert.IsEmpty(ndSets);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledNonDietary_TestSurveysAndExposures(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );

            var ndSets = GetAllNonDietaryExposureSets(managerType);

            Assert.HasCount(7, ndSets);
            Assert.IsTrue(ndSets.All(s => string.IsNullOrWhiteSpace(s.Code)));
            Assert.AreEqual(
                "S1-1-A;S1-1-B;S1-2-C;S1-2-D;S1-3-A;S1-3-D;S1-4-B;S1-4-D;S2-4-C;S2-4-E;S2-5-A;S2-5-F;S2-6-C;S2-6-F",
                string.Join(";", ndSets
                .SelectMany(s => s.NonDietaryExposures,
                           (s, e) => $"{s.NonDietarySurvey.Code}-{e.IdIndividual}-{e.Compound.Code}"))
            );
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledNonDietary_TestSurveysAndExposuresFilterCompoundsTest(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var ndSets = GetAllNonDietaryExposureSets(managerType);

            Assert.HasCount(5, ndSets);
            Assert.IsTrue(ndSets.All(s => string.IsNullOrWhiteSpace(s.Code)));
            Assert.AreEqual(
                "S1-1-B;S1-2-C;S1-4-B;S2-4-C;S2-6-C",
                string.Join(";", ndSets
                .SelectMany(s => s.NonDietaryExposures,
                           (s, e) => $"{s.NonDietarySurvey.Code}-{e.IdIndividual}-{e.Compound.Code}"))
            );
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledNonDietary_TestFilterSurvey(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );
            RawDataProvider.SetFilterCodes(ScopingType.NonDietarySurveys, ["s2"]);

            var ndSets = GetAllNonDietaryExposureSets(managerType);

            Assert.HasCount(3, ndSets);
            Assert.IsTrue(ndSets.All(s => string.IsNullOrWhiteSpace(s.Code)));
            Assert.AreEqual(
                "S2-4-C;S2-4-E;S2-5-A;S2-5-F;S2-6-C;S2-6-F",
                string.Join(";", ndSets
                .SelectMany(s => s.NonDietaryExposures,
                           (s, e) => $"{s.NonDietarySurvey.Code}-{e.IdIndividual}-{e.Compound.Code}"))
            );
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledNonDietary_TestCombineWithIndividualsTest(ManagerType managerType) {
            RawDataProvider.SetEmptyDataSource(SourceTableGroup.Survey);
            RawDataProvider.SetDataTables(
                (ScopingType.NonDietarySurveys, @"NonDietaryTests/NonDietarySurveys"),
                (ScopingType.NonDietaryExposures, @"NonDietaryTests/NonDietaryExposures")
            );

            var ndSets = GetAllNonDietaryExposureSets(managerType);

            Assert.HasCount(7, ndSets);
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
