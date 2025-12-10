using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledEffectsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffects_TestGetAllEffects(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Effects, @"EffectsTests/EffectsSimple")
            );

            var effects = GetAllEffects(managerType);

            Assert.HasCount(4, effects);
            for (int i = 1; i < 5; i++) {
                Assert.IsTrue(effects.TryGetValue($"EFF{i}", out var effect) && effect.Name.Equals($"Effect {i}"));
            }
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledEffects_TestGetAllEffectsFiltered(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Effects, @"EffectsTests/EffectsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["EFF1", "EFF3"]);

            var effects = GetAllEffects(managerType);

            Assert.HasCount(2, effects);
            Assert.IsTrue(effects.TryGetValue("eff1", out var effect) && effect.Name.Equals("Effect 1"));
            Assert.IsTrue(effects.TryGetValue("eff3", out effect) && effect.Name.Equals("Effect 3"));
        }
    }
}
