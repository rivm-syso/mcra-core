using MCRA.General.Sbml;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Test.UnitTests.KineticModelDefinitions.SbmlPbkExtensions {
    [TestClass]
    public class SbmlUnitDefinitionExtensions {

        [TestMethod]
        [DataRow(1, 0, TimeUnit.Seconds)]
        [DataRow(60D, 0, TimeUnit.Minutes)]
        [DataRow(3600D, 0, TimeUnit.Hours)]
        [DataRow(86400, 0, TimeUnit.Days)]
        [DataRow(8.64, 4, TimeUnit.Days)]
        public void SbmlUnitDefinitionExtensions_TestToTimeUnit(
            double multiplier,
            double scale,
            TimeUnit expected
        ) {
            var unitDefinition = createUnitDefinition(
                SbmlUnitKind.Second,
                multiplier,
                scale
            );
            var result = unitDefinition.ToTimeUnit();
            Assert.AreEqual(result, expected);
        }

        private static SbmlUnitDefinition createUnitDefinition(
            SbmlUnitKind kind,
            double multiplier,
            double scale
        ) {
            return new SbmlUnitDefinition() {
                Units = [
                    new SbmlUnit() {
                        Exponent = 1,
                        Kind = kind,
                        Scale = (decimal)scale,
                        Multiplier = (decimal)multiplier
                    }
                ]
            };
        }
    }
}
