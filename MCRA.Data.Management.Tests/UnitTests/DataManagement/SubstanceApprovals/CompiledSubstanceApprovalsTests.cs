using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledSubstanceApprovalsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledSubstanceApprovals_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.SubstanceApprovals, @"SubstanceApprovalsTests/SubstanceApprovalsSimple")
            );

            var records = GetAllSubstanceApprovals(managerType);

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct().ToList();
            var approvals = records.Select(f => f.IsApproved).ToList();

            CollectionAssert.AreEquivalent(new[] { "SubA", "SubB", "SubC", "SubD", "SubE" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { true, false, true, true, false }, approvals);
        }
    }
}
