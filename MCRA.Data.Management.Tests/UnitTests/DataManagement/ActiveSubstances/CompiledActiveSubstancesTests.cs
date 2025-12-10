using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledActiveSubstancesTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledActiveSubstances_TestModelsOnly(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels")
            );

            var models = GetAllActiveSubstanceModels(managerType);

            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, models.Keys.ToList());

            Assert.IsEmpty(models["agm1"].MembershipProbabilities);
            Assert.IsEmpty(models["agm2"].MembershipProbabilities);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledActiveSubstances_TestSimpleEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels")
            );

            //set a filter scope on effects
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["eff2"]);

            var models = GetAllActiveSubstanceModels(managerType);

            CollectionAssert.AreEqual(new[] { "Agm2" }, models.Keys.ToList());

            Assert.IsEmpty(models["agm2"].MembershipProbabilities);
            Assert.AreEqual("eff2", models["agm2"].Effect.Code.ToLower());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledActiveSubstances_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );

            var models = GetAllActiveSubstanceModels(managerType);

            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm1"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, compoundCodes);

            compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "E" }, compoundCodes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledActiveSubstances_TestEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );

            //set a filter scope on effects
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["eff2"]);

            var models = GetAllActiveSubstanceModels(managerType);

            CollectionAssert.AreEqual(new[] { "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "E" }, compoundCodes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledActiveSubstances_TestEffectsCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );

            //set a filter scope on effects
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["eff2"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var models = GetAllActiveSubstanceModels(managerType);

            CollectionAssert.AreEqual(new[] { "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledActiveSubstances_TestCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );

            //set a filter scope on compounds
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var models = GetAllActiveSubstanceModels(managerType);

            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm1"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);

            compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }
    }
}
