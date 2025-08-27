using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledQsarMembershipModelsTests : CompiledTestsBase {

        protected Func<IDictionary<string, QsarMembershipModel>> _getItemsDelegate;

        [TestMethod]
        public void CompiledQsarMembershipModels_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple")
            );

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, models.Keys.ToList());
        }


        [TestMethod]
        public void CompiledQsarMembershipModels_TestSimpleEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, models.Keys.ToList());

        }

        [TestMethod]
        public void CompiledQsarMembershipModels_TestMembershipScoresSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple"),
                (ScopingType.QsarMembershipScores, @"QsarMembershipModelsTests/QsarMembershipScoresSimple")
            );

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "MD1", "MD2", "MD3" }, models.Keys.ToList());

            var compoundCodes = models["MD1"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, compoundCodes);

            compoundCodes = models["MD2"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "D" }, compoundCodes);

            compoundCodes = models["MD3"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
        }

        [TestMethod]
        public void CompiledQsarMembershipModels_TestMembershipScoresFilterEffectsAndCompounds() {
            _rawDataProvider.SetDataTables(
                (ScopingType.QsarMembershipModels, @"QsarMembershipModelsTests/QsarMembershipModelsSimple"),
                (ScopingType.QsarMembershipScores, @"QsarMembershipModelsTests/QsarMembershipScoresSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B", "D"]);

            var models = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "MD1", "MD3" }, models.Keys.ToList());

            var compoundCodes = models["MD1"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, compoundCodes);

            compoundCodes = models["MD3"].MembershipScores.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
        }
    }
}
