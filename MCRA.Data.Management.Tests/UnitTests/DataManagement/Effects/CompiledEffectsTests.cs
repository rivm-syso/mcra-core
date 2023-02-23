using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledEffectsTests : CompiledTestsBase {
        protected Func<IDictionary<string, Effect>> _getEffectsDelegate;

        [TestMethod]
        public void CompiledEffects_TestGetAllEffects() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Effects, @"EffectsTests\EffectsSimple")
            );

            var effects = _getEffectsDelegate.Invoke();

            Assert.AreEqual(4, effects.Count);
            Effect effect;
            for (int i = 1; i < 5; i++) {
                Assert.IsTrue(effects.TryGetValue($"EFF{i}", out effect) && effect.Name.Equals($"Effect {i}"));
            }
        }

        [TestMethod]
        public void CompiledEffects_TestGetAllEffectsFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Effects, @"EffectsTests\EffectsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "EFF1", "EFF3" });

            var effects = _getEffectsDelegate.Invoke();

            Assert.AreEqual(2, effects.Count);
            Effect effect;
            Assert.IsTrue(effects.TryGetValue("eff1", out effect) && effect.Name.Equals("Effect 1"));
            Assert.IsTrue(effects.TryGetValue("eff3", out effect) && effect.Name.Equals("Effect 3"));
        }
    }
}
