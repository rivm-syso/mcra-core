using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledQsarMembershipModelsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledQsarMembershipModels_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple")
            );

            var models = GetAllQsarMembershipModels(managerType);

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, models.Keys.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledQsarMembershipModels_TestSimpleEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var models = GetAllQsarMembershipModels(managerType);

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, models.Keys.ToList());

        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledQsarMembershipModels_TestMembershipScoresSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple"),
                (ScopingType.QsarMembershipScores, @"QsarMembershipModelsTests/QsarMembershipScoresSimple")
            );

            var models = GetAllQsarMembershipModels(managerType);

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, models.Keys.ToList());

            var compoundCodes = models["MD1"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, compoundCodes);

            compoundCodes = models["MD2"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "D" }, compoundCodes);

            compoundCodes = models["MD3"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledQsarMembershipModels_TestMembershipScoresFilterEffectsAndCompounds(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple"),
                (ScopingType.QsarMembershipScores, @"QsarMembershipModelsTests/QsarMembershipScoresSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B", "D"]);

            var models = GetAllQsarMembershipModels(managerType);

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, models.Keys.ToList());

            var compoundCodes = models["MD1"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, compoundCodes);

            compoundCodes = models["MD3"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
        }
    }
}
