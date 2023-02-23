using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledActiveSubstancesTests : CompiledTestsBase {
        protected Func<IDictionary<string, ActiveSubstanceModel>> _getItemsDelegate;

        public void CompiledActiveSubstances_TestModelsOnly() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests\AssessmentGroupMembershipModels")
            );

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, models.Keys.ToList());

            Assert.AreEqual(0, models["agm1"].MembershipProbabilities.Count);
            Assert.AreEqual(0, models["agm2"].MembershipProbabilities.Count);
        }

        [TestMethod]
        public void CompiledActiveSubstances_TestSimpleEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests\AssessmentGroupMembershipModels")
            );

            //set a filter scope on effects
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "eff2" });

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "Agm2" }, models.Keys.ToList());

            Assert.AreEqual(0, models["agm2"].MembershipProbabilities.Count);
            Assert.AreEqual("eff2", models["agm2"].Effect.Code.ToLower());
        }

        [TestMethod]
        public void CompiledActiveSubstances_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests\AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests\AssessmentGroupMemberships")
            );

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm1"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, compoundCodes);

            compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "E" }, compoundCodes);
        }

        [TestMethod]
        public void CompiledActiveSubstances_TestEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests\AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests\AssessmentGroupMemberships")
            );

            //set a filter scope on effects
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "eff2" });

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "E" }, compoundCodes);
        }

        [TestMethod]
        public void CompiledActiveSubstances_TestEffectsCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests\AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests\AssessmentGroupMemberships")
            );

            //set a filter scope on effects
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "eff2" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }

        [TestMethod]
        public void CompiledActiveSubstances_TestCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests\AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests\AssessmentGroupMemberships")
            );

            //set a filter scope on compounds
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, models.Keys.ToList());

            var compoundCodes = models["agm1"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);

            compoundCodes = models["agm2"].MembershipProbabilities.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }
    }
}
