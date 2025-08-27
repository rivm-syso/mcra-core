using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledSubstanceApprovalsTests : CompiledTestsBase {
        protected Func<ICollection<SubstanceApproval>> _getItemsDelegate;

        [TestMethod]
        public void CompiledSubstanceApprovals_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceApprovals, @"SubstanceApprovalsTests/SubstanceApprovalsSimple")
            );

            var records = _getItemsDelegate.Invoke();

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct().ToList();
            var approvals = records.Select(f => f.IsApproved).ToList();

            CollectionAssert.AreEquivalent(new[] { "SubA", "SubB", "SubC", "SubD", "SubE" }, compoundCodes);
            CollectionAssert.AreEquivalent(new[] { true, false, true, true, false }, approvals);
        }
    }
}
