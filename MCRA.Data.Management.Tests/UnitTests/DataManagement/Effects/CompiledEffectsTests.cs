using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledEffectsTests : CompiledTestsBase {
        protected Func<IDictionary<string, Effect>> _getEffectsDelegate;

        [TestMethod]
        public void CompiledEffects_TestGetAllEffects() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Effects, @"EffectsTests/EffectsSimple")
            );

            var effects = _getEffectsDelegate.Invoke();

            Assert.AreEqual(4, effects.Count);
            for (int i = 1; i < 5; i++) {
                Assert.IsTrue(effects.TryGetValue($"EFF{i}", out var effect) && effect.Name.Equals($"Effect {i}"));
            }
        }

        [TestMethod]
        public void CompiledEffects_TestGetAllEffectsFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Effects, @"EffectsTests/EffectsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["EFF1", "EFF3"]);

            var effects = _getEffectsDelegate.Invoke();

            Assert.AreEqual(2, effects.Count);
            Assert.IsTrue(effects.TryGetValue("eff1", out var effect) && effect.Name.Equals("Effect 1"));
            Assert.IsTrue(effects.TryGetValue("eff3", out effect) && effect.Name.Equals("Effect 3"));
        }
    }
}
