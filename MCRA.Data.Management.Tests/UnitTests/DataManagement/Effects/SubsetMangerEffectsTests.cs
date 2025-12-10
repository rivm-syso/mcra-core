using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    [TestClass]
    public class SubsetMangerEffectsTests : CompiledTestsBase {
        [TestMethod]
        public void SubsetManager_TestGetSelectedEffect() {
            RawDataProvider.SetDataTables((ScopingType.Effects, @"EffectsTests/EffectsSimple"));

            var selectedEffect = SubsetManager.SelectedEffect;
            Assert.IsNull(selectedEffect);
            var config = Project.EffectsSettings;
            config.IncludeAopNetwork = true;
            config.CodeFocalEffect = "EFF2";

            selectedEffect = SubsetManager.SelectedEffect;
            Assert.AreEqual("Eff2", selectedEffect.Code);
            Assert.AreEqual(SubsetManager.AllEffects["EFF2"], selectedEffect);
        }

        /// <summary>
        /// Tests correct loading of the effects. Validation check: assert that the expected
        /// effects are present in the effects of the compiled datasource and no other
        /// effects exist in the compiled datasource.
        /// </summary>
        [TestMethod]
        public void SubsetManager_TestGetAllEffects() {
            RawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Effects);

            var effects = CompiledDataManager.GetAllEffects();
            var effectCodes = effects.Keys.ToList();
            CollectionAssert.AreEquivalent(new List<string>() { "EffectGroup1", "EffectGroup2" }, effectCodes);
        }

        /// <summary>
        /// Tests correct loading of the relative potency factors. Validation check: assert that the
        /// expected compounds of the effect exist and no other compounds exist in the effects.
        /// </summary>
        [TestMethod]
        public void SubsetManager_TestGetAllRelativePotencyFactors() {
            RawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.Compounds, SourceTableGroup.Effects, SourceTableGroup.RelativePotencyFactors]);

            var effects = CompiledDataManager.GetAllEffects();
            var effectGroup1 = effects["EffectGroup1"];
            var effectGroup2 = effects["EffectGroup2"];

            var relativePotencyFactors = CompiledDataManager.GetAllRelativePotencyFactors();
            var compoundCodesEffectGroup1 = relativePotencyFactors[effectGroup1.Code].Select(r => r.Compound).ToList();
            var compoundCodesEffectGroup2 = relativePotencyFactors[effectGroup2.Code].Select(r => r.Compound).ToList();

            var compounds = CompiledDataManager.GetAllCompounds();
            var compoundA = compounds["CompoundA"];
            var compoundB = compounds["CompoundB"];
            var compoundC = compounds["CompoundC"];
            var compoundD = compounds["CompoundD"];

            CollectionAssert.AreEquivalent(new List<Compound>() { compoundA, compoundB, compoundC, compoundD }, compoundCodesEffectGroup1);
            CollectionAssert.AreEquivalent(new List<Compound>() { compoundA, compoundB }, compoundCodesEffectGroup2);
        }
    }
}
