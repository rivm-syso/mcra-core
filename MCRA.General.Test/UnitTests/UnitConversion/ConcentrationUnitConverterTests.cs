using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {

    [TestClass]
    public class ConcentrationUnitConverterTests {

        /// <summary>
        /// Tests whether there is a value definition for each enum value of the amount units enum type.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestCompleteness() {
            var definition = ConcentrationUnitConverter.UnitDefinition;
            var enumValues = Enum.GetValues(typeof(ConcentrationUnit)).Cast<ConcentrationUnit>();
            // Check whether there is a unit definition for each enum value.
            foreach (var value in enumValues) {
                var unitValueDefinition = definition.FromString<ConcentrationUnit>(value.ToString());
                Assert.AreEqual(unitValueDefinition, value);
            }
            // Check whether each unit specified in the definitions matches an enum value.
            foreach (var units in definition.Units) {
                var value = Enum.Parse(typeof(ConcentrationUnit), units.Id);
            }
        }

        /// <summary>
        /// Tests whether each of the specified aliases is correctly parsed.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestAliases() {
            var definition = ConcentrationUnitConverter.UnitDefinition;
            var aliases = definition.Units.SelectMany(r => r.Aliases, (r, a) => new {
                ConcentrationUnit = Enum.Parse(typeof(ConcentrationUnit), r.Id),
                Alias = a
            });
            // Check whether the parsed unit for each alias matches with the amount unit for which it is supposed to be an alias.
            foreach (var alias in aliases) {
                var parsedUnit = ConcentrationUnitConverter.FromString(alias.Alias);
                Assert.AreEqual(alias.ConcentrationUnit, parsedUnit);
            }
        }

        /// <summary>
        /// Tests whether there is a display name and a short display name for each unit.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestDisplayNames() {
            var definition = ConcentrationUnitConverter.UnitDefinition;
            foreach (var units in definition.Units) {
                var value = (ConcentrationUnit)Enum.Parse(typeof(ConcentrationUnit), units.Id);
                Assert.IsTrue(units.Name.Equals(value.GetDisplayName(), StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(units.ShortName.Equals(value.GetShortDisplayName(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Tests whether there is a concentration amount multiplier for each unit.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestGetLog10ConcentrationAmountMultiplier() {
            var enumValues = Enum.GetValues(typeof(ConcentrationUnit)).Cast<ConcentrationUnit>();
            foreach (var value in enumValues) {
                var multiplier = ConcentrationUnitExtensions.GetLog10ConcentrationAmountMultiplier(value);
                Assert.IsFalse(double.IsNaN(multiplier));
            }
        }

        /// <summary>
        /// Test concentration unit conversion for all known types, including the specified default
        /// concentration unit type in case the string is null or empty.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestFromString() {
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kg/kg"), ConcentrationUnit.kgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kilogram/kilogram"), ConcentrationUnit.kgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kilogram/kg"), ConcentrationUnit.kgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("0"), ConcentrationUnit.kgPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("g/kg"), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gram/kilogram"), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gram/kg"), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G015A"), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-3"), ConcentrationUnit.gPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("mg/kg"), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligram/kilogram"), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligram/kg"), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligr/kg"), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G061A"), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G049A"), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-6"), ConcentrationUnit.mgPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("µg/kg"), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgram/kilogram"), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgram/kg"), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgr/kg"), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G050A"), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-9"), ConcentrationUnit.ugPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("ng/kg"), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogram/kilogram"), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogram/kg"), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogr/kg"), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G077A"), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-12"), ConcentrationUnit.ngPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("pg/kg"), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogram/kilogram"), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogram/kg"), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogr/kg"), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G081A"), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-15"), ConcentrationUnit.pgPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("kg/L"), ConcentrationUnit.kgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kg/l"), ConcentrationUnit.kgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kilogram/liter"), ConcentrationUnit.kgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kilogram/litre"), ConcentrationUnit.kgPerL);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("g/L"), ConcentrationUnit.gPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("g/l"), ConcentrationUnit.gPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gram/liter"), ConcentrationUnit.gPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gram/litre"), ConcentrationUnit.gPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gr/l"), ConcentrationUnit.gPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gr/L"), ConcentrationUnit.gPerL);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("mg/L"), ConcentrationUnit.mgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("mg/l"), ConcentrationUnit.mgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligram/liter"), ConcentrationUnit.mgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligram/litre"), ConcentrationUnit.mgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligr/l"), ConcentrationUnit.mgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligr/L"), ConcentrationUnit.mgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G052A"), ConcentrationUnit.mgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G062A"), ConcentrationUnit.mgPerL);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("µg/L"), ConcentrationUnit.ugPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("µg/l"), ConcentrationUnit.ugPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgram/liter"), ConcentrationUnit.ugPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgram/litre"), ConcentrationUnit.ugPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgr/L"), ConcentrationUnit.ugPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgr/l"), ConcentrationUnit.ugPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G051A"), ConcentrationUnit.ugPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G079A"), ConcentrationUnit.ugPerL);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("ng/L"), ConcentrationUnit.ngPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("ng/l"), ConcentrationUnit.ngPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogram/liter"), ConcentrationUnit.ngPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogram/litre"), ConcentrationUnit.ngPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogr/l"), ConcentrationUnit.ngPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogr/L"), ConcentrationUnit.ngPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G078A"), ConcentrationUnit.ngPerL);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("pg/L"), ConcentrationUnit.pgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("pg/l"), ConcentrationUnit.pgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogram/liter"), ConcentrationUnit.pgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogram/litre"), ConcentrationUnit.pgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogr/L"), ConcentrationUnit.pgPerL);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogr/l"), ConcentrationUnit.pgPerL);

            try {
                var unit = ConcentrationUnitConverter.FromString("askdha");
                Assert.Fail();
            } catch (Exception) {
            }
        }

        /// <summary>
        /// Test concentration unit conversion for all known types, including the specified default
        /// concentration unit type in case the string is null or empty.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestFromStringWithDefault() {
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kg/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.kgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kilogram/kilogram", ConcentrationUnit.kgPerKg), ConcentrationUnit.kgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("kilogram/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.kgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("0", ConcentrationUnit.kgPerKg), ConcentrationUnit.kgPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("g/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gram/kilogram", ConcentrationUnit.kgPerKg), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("gram/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G015A", ConcentrationUnit.kgPerKg), ConcentrationUnit.gPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-3", ConcentrationUnit.kgPerKg), ConcentrationUnit.gPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("mg/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligram/kilogram", ConcentrationUnit.kgPerKg), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligram/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("milligr/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G061A", ConcentrationUnit.kgPerKg), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G049A", ConcentrationUnit.kgPerKg), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-6", ConcentrationUnit.kgPerKg), ConcentrationUnit.mgPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("µg/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgram/kilogram", ConcentrationUnit.kgPerKg), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgram/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("microgr/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G050A", ConcentrationUnit.kgPerKg), ConcentrationUnit.ugPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-9", ConcentrationUnit.kgPerKg), ConcentrationUnit.ugPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("ng/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogram/kilogram", ConcentrationUnit.kgPerKg), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogram/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("nanogr/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G077A", ConcentrationUnit.kgPerKg), ConcentrationUnit.ngPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-12", ConcentrationUnit.kgPerKg), ConcentrationUnit.ngPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString("pg/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogram/kilogram", ConcentrationUnit.kgPerKg), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogram/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("picogr/kg", ConcentrationUnit.kgPerKg), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("G081A", ConcentrationUnit.kgPerKg), ConcentrationUnit.pgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString("-15", ConcentrationUnit.kgPerKg), ConcentrationUnit.pgPerKg);

            Assert.AreEqual(ConcentrationUnitConverter.FromString(string.Empty, ConcentrationUnit.kgPerKg), ConcentrationUnit.kgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString(string.Empty, ConcentrationUnit.mgPerKg), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(ConcentrationUnitConverter.FromString(string.Empty, ConcentrationUnit.pgPerKg), ConcentrationUnit.pgPerKg);

            try {
                var unit = ConcentrationUnitConverter.FromString("askdha");
                Assert.Fail();
            } catch (Exception) {
            }
        }

        /// <summary>
        /// Tests whether there is a value definition for each enum value of the amount units enum type.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestParallel() {
            var values = Enumerable.Range(1, 100000)
                .AsParallel()
                .Select(r => ConcentrationUnitConverter.FromString("G061A"))
                .ToList();
        }

        /// <summary>
        /// Test whether the alignment factor to align a concentration with a target unit
        /// is computed correctly.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestGetAlignmentFactor() {
            var targetUnit = new ExposureUnitTriple(SubstanceAmountUnit.Milligrams, ConcentrationMassUnit.Kilograms, TimeScaleUnit.PerDay);
            Assert.AreEqual(1D, ConcentrationUnit.mgPerKg.GetConcentrationAlignmentFactor(targetUnit, double.NaN));
            Assert.AreEqual(1e-3, ConcentrationUnit.ugPerKg.GetConcentrationAlignmentFactor(targetUnit, double.NaN));
            Assert.AreEqual(1e3, ConcentrationUnit.gPerKg.GetConcentrationAlignmentFactor(targetUnit, double.NaN));
        }

        /// <summary>
        /// Test whether the unit multiplication factor to align a concentration with a specified
        /// target concentration unit is computed correctly for some concentration unit pairs.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestGetConcentrationUnitMultiplier() {
            Assert.AreEqual(1D, ConcentrationUnit.mgPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.mgPerKg), 1e-10);
            Assert.AreEqual(1e3, ConcentrationUnit.mgPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.ugPerKg), 1e-10);
            Assert.AreEqual(1e3, ConcentrationUnit.mgPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.ugPerL), 1e-10);
            Assert.AreEqual(1e-3, ConcentrationUnit.mgPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.gPerL), 1e-10);
            Assert.AreEqual(1e-3, ConcentrationUnit.mgPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.gPerKg), 1e-10);
            Assert.AreEqual(1D, ConcentrationUnit.gPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.gPerKg), 1e-10);
            Assert.AreEqual(1D, ConcentrationUnit.kgPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.kgPerKg), 1e-10);
            Assert.AreEqual(1e-3, ConcentrationUnit.gPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.kgPerKg), 1e-10);
            Assert.AreEqual(1e3, ConcentrationUnit.kgPerKg.GetConcentrationUnitMultiplier(ConcentrationUnit.gPerKg), 1e-10);
        }

        /// <summary>
        /// Tests for valid substance amount unit extraction for some selected concentration units.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestGetSubstanceAmountUnit() {
            Assert.AreEqual(SubstanceAmountUnit.Grams, ConcentrationUnit.gPerKg.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Kilograms, ConcentrationUnit.kgPerKg.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Milligrams, ConcentrationUnit.mgPerKg.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Nanograms, ConcentrationUnit.ngPerL.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ConcentrationUnit.ugPermL.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ConcentrationUnit.ugPerg.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Nanograms, ConcentrationUnit.ngPerKg.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ConcentrationUnit.ugPermL.GetSubstanceAmountUnit());
            Assert.AreEqual(SubstanceAmountUnit.Micrograms, ConcentrationUnit.ugPerL.GetSubstanceAmountUnit());
        }

        /// <summary>
        /// Tests whether a substance amount can be obtained for all concentration units.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestGetSubstanceAmountUnitAll() {
            var enumValues = Enum.GetValues(typeof(ConcentrationUnit)).Cast<ConcentrationUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetSubstanceAmountUnit() == SubstanceAmountUnit.Undefined);
            }
        }

        /// <summary>
        /// Tests for valid concentration mass unit extraction for some selected concentration units.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestGetConcentrationMassUnit() {
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ConcentrationUnit.kgPerKg.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ConcentrationUnit.gPerKg.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ConcentrationUnit.ugPerKg.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ConcentrationUnit.ngPerKg.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Kilograms, ConcentrationUnit.pgPerKg.GetConcentrationMassUnit());

            Assert.AreEqual(ConcentrationMassUnit.Liter, ConcentrationUnit.kgPerL.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Liter, ConcentrationUnit.gPerL.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Liter, ConcentrationUnit.ugPerL.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Liter, ConcentrationUnit.ngPerL.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Liter, ConcentrationUnit.pgPerL.GetConcentrationMassUnit());

            Assert.AreEqual(ConcentrationMassUnit.Milliliter, ConcentrationUnit.ugPermL.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Milliliter, ConcentrationUnit.ngPermL.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, ConcentrationUnit.ngPerg.GetConcentrationMassUnit());
            Assert.AreEqual(ConcentrationMassUnit.Grams, ConcentrationUnit.ugPerg.GetConcentrationMassUnit());
        }

        /// <summary>
        /// Tests whether a concentration mass unit can be obtained for all concentration units.
        /// </summary>
        [TestMethod]
        public void ConcentrationUnitConverter_TestGetConcentrationMassUnitAll() {
            var enumValues = Enum.GetValues(typeof(ConcentrationUnit)).Cast<ConcentrationUnit>().ToList();
            foreach (var value in enumValues) {
                Assert.IsFalse(value.GetConcentrationMassUnit() == ConcentrationMassUnit.Undefined);
                Assert.IsFalse(value.GetConcentrationMassUnit() == ConcentrationMassUnit.PerUnit);
            }
        }
    }
}
