using MCRA.Utils.Sbml.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Sbml {

    [TestClass]
    public class SbmlUnitDefinitionTests {

        [TestMethod]
        [DataRow(SbmlUnitKind.Gram, 1, 1, 0, "g")]
        [DataRow(SbmlUnitKind.Gram, 1, 1, -6, "ug")]
        [DataRow(SbmlUnitKind.Gram, -1, 1, 0, "/g")]
        [DataRow(SbmlUnitKind.Mole, 1, 1, 0, "mol")]
        [DataRow(SbmlUnitKind.Mole, 1, 1, -6, "umol")]
        [DataRow(SbmlUnitKind.Metre, 1, 1, 0, "m")]
        [DataRow(SbmlUnitKind.Metre, 2, 1, 0, "m^2")]
        [DataRow(SbmlUnitKind.Litre, 1, 1, 0, "L")]
        [DataRow(SbmlUnitKind.Litre, 1, 1, 1, "daL")]
        [DataRow(SbmlUnitKind.Second, 1, 1, 0, "s")]
        [DataRow(SbmlUnitKind.Second, 1, 60, 0, "min")]
        [DataRow(SbmlUnitKind.Second, 1, 3600, 0, "h")]
        [DataRow(SbmlUnitKind.Second, 1, 86400, 0, "d")]
        [DataRow(SbmlUnitKind.Dimensionless, 3, 4, -2, "dimensionless")]
        public void SbmlUnitDefinition_TestSimpleUnits(
            SbmlUnitKind kind,
            int exponent,
            int multiplier,
            int scale,
            string expected
        ) {
            var def = new SbmlUnitDefinition() {
                Units = [
                    new() {
                        Kind = kind,
                        Exponent = exponent,
                        Multiplier = multiplier,
                        Scale = scale
                    }
                ]
            };
            var unitString = def.GetUnitString();
            Assert.AreEqual(expected, unitString);
        }

        [TestMethod]
        [DataRow(SbmlUnitKind.Gram, 1, 1, 0, SbmlUnitKind.Litre, -1, 1, 0, "g/L")]
        [DataRow(SbmlUnitKind.Gram, 1, 1, 0, SbmlUnitKind.Second, -1, 3600, 0, "g/h")]
        [DataRow(SbmlUnitKind.Gram, 1, 1, -3, SbmlUnitKind.Litre, -1, 1, -3, "mg/mL")]
        [DataRow(SbmlUnitKind.Mole, 1, 1, 0, SbmlUnitKind.Litre, -1, 1, 0, "mol/L")]
        [DataRow(SbmlUnitKind.Mole, -1, 1, 0, SbmlUnitKind.Litre, -1, 1, 0, "/mol/L")]
        [DataRow(SbmlUnitKind.Dimensionless, 1, 1, 0, SbmlUnitKind.Litre, -1, 1, 0, "/L")]
        [DataRow(SbmlUnitKind.Dimensionless, 1, 1, 0, SbmlUnitKind.Litre, 1, 1, 0, "L")]
        public void SbmlUnitDefinition_TestTwoPartUnits(
            SbmlUnitKind kindFirst,
            int exponentFirst,
            int multiplierFirst,
            int scaleFirst,
            SbmlUnitKind kindSecond,
            int exponentSecond,
            int multiplierSecond,
            int scaleSecond,
            string expected
        ) {
            var def = new SbmlUnitDefinition() {
                Units = [
                    new() {
                        Kind = kindFirst,
                        Exponent = exponentFirst,
                        Multiplier = multiplierFirst,
                        Scale = scaleFirst
                    },
                    new() {
                        Kind = kindSecond,
                        Exponent = exponentSecond,
                        Multiplier = multiplierSecond,
                        Scale = scaleSecond
                    }
                ]
            };
            var unitString = def.GetUnitString();
            Assert.AreEqual(expected, unitString);
        }
    }
}
