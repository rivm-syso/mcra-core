using MCRA.Utils.ExtensionMethods;

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
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("kg/kg"));
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("kilogram/kilogram"));
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("kilogram/kg"));
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("0"));

            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("g/kg"));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("gram/kilogram"));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("gram/kg"));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("G015A"));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("-3"));

            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("mg/kg"));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("milligram/kilogram"));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("milligram/kg"));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("milligr/kg"));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("G061A"));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("G049A"));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("-6"));

            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("µg/kg"));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("microgram/kilogram"));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("microgram/kg"));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("microgr/kg"));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("G050A"));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("-9"));

            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("ng/kg"));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("nanogram/kilogram"));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("nanogram/kg"));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("nanogr/kg"));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("G077A"));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("-12"));

            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("pg/kg"));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("picogram/kilogram"));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("picogram/kg"));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("picogr/kg"));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("G081A"));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("-15"));

            Assert.AreEqual(ConcentrationUnit.kgPerL, ConcentrationUnitConverter.FromString("kg/L"));
            Assert.AreEqual(ConcentrationUnit.kgPerL, ConcentrationUnitConverter.FromString("kg/l"));
            Assert.AreEqual(ConcentrationUnit.kgPerL, ConcentrationUnitConverter.FromString("kilogram/liter"));
            Assert.AreEqual(ConcentrationUnit.kgPerL, ConcentrationUnitConverter.FromString("kilogram/litre"));

            Assert.AreEqual(ConcentrationUnit.gPerL, ConcentrationUnitConverter.FromString("g/L"));
            Assert.AreEqual(ConcentrationUnit.gPerL, ConcentrationUnitConverter.FromString("g/l"));
            Assert.AreEqual(ConcentrationUnit.gPerL, ConcentrationUnitConverter.FromString("gram/liter"));
            Assert.AreEqual(ConcentrationUnit.gPerL, ConcentrationUnitConverter.FromString("gram/litre"));
            Assert.AreEqual(ConcentrationUnit.gPerL, ConcentrationUnitConverter.FromString("gr/l"));
            Assert.AreEqual(ConcentrationUnit.gPerL, ConcentrationUnitConverter.FromString("gr/L"));

            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("mg/L"));
            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("mg/l"));
            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("milligram/liter"));
            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("milligram/litre"));
            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("milligr/l"));
            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("milligr/L"));
            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("G052A"));
            Assert.AreEqual(ConcentrationUnit.mgPerL, ConcentrationUnitConverter.FromString("G062A"));

            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("µg/L"));
            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("µg/l"));
            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("microgram/liter"));
            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("microgram/litre"));
            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("microgr/L"));
            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("microgr/l"));
            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("G051A"));
            Assert.AreEqual(ConcentrationUnit.ugPerL, ConcentrationUnitConverter.FromString("G079A"));

            Assert.AreEqual(ConcentrationUnit.ngPerL, ConcentrationUnitConverter.FromString("ng/L"));
            Assert.AreEqual(ConcentrationUnit.ngPerL, ConcentrationUnitConverter.FromString("ng/l"));
            Assert.AreEqual(ConcentrationUnit.ngPerL, ConcentrationUnitConverter.FromString("nanogram/liter"));
            Assert.AreEqual(ConcentrationUnit.ngPerL, ConcentrationUnitConverter.FromString("nanogram/litre"));
            Assert.AreEqual(ConcentrationUnit.ngPerL, ConcentrationUnitConverter.FromString("nanogr/l"));
            Assert.AreEqual(ConcentrationUnit.ngPerL, ConcentrationUnitConverter.FromString("nanogr/L"));
            Assert.AreEqual(ConcentrationUnit.ngPerL, ConcentrationUnitConverter.FromString("G078A"));

            Assert.AreEqual(ConcentrationUnit.pgPerL, ConcentrationUnitConverter.FromString("pg/L"));
            Assert.AreEqual(ConcentrationUnit.pgPerL, ConcentrationUnitConverter.FromString("pg/l"));
            Assert.AreEqual(ConcentrationUnit.pgPerL, ConcentrationUnitConverter.FromString("picogram/liter"));
            Assert.AreEqual(ConcentrationUnit.pgPerL, ConcentrationUnitConverter.FromString("picogram/litre"));
            Assert.AreEqual(ConcentrationUnit.pgPerL, ConcentrationUnitConverter.FromString("picogr/L"));
            Assert.AreEqual(ConcentrationUnit.pgPerL, ConcentrationUnitConverter.FromString("picogr/l"));

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
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("kg/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("kilogram/kilogram", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("kilogram/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString("0", ConcentrationUnit.kgPerKg));

            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("g/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("gram/kilogram", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("gram/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("G015A", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.gPerKg, ConcentrationUnitConverter.FromString("-3", ConcentrationUnit.kgPerKg));

            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("mg/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("milligram/kilogram", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("milligram/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("milligr/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("G061A", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("G049A", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString("-6", ConcentrationUnit.kgPerKg));

            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("µg/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("microgram/kilogram", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("microgram/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("microgr/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("G050A", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ugPerKg, ConcentrationUnitConverter.FromString("-9", ConcentrationUnit.kgPerKg));

            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("ng/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("nanogram/kilogram", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("nanogram/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("nanogr/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("G077A", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.ngPerKg, ConcentrationUnitConverter.FromString("-12", ConcentrationUnit.kgPerKg));

            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("pg/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("picogram/kilogram", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("picogram/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("picogr/kg", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("G081A", ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString("-15", ConcentrationUnit.kgPerKg));

            Assert.AreEqual(ConcentrationUnit.kgPerKg, ConcentrationUnitConverter.FromString(string.Empty, ConcentrationUnit.kgPerKg));
            Assert.AreEqual(ConcentrationUnit.mgPerKg, ConcentrationUnitConverter.FromString(string.Empty, ConcentrationUnit.mgPerKg));
            Assert.AreEqual(ConcentrationUnit.pgPerKg, ConcentrationUnitConverter.FromString(string.Empty, ConcentrationUnit.pgPerKg));

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
                Assert.AreNotEqual(SubstanceAmountUnit.Undefined, value.GetSubstanceAmountUnit());
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
                Assert.AreNotEqual(ConcentrationMassUnit.Undefined, value.GetConcentrationMassUnit());
                Assert.AreNotEqual(ConcentrationMassUnit.PerUnit, value.GetConcentrationMassUnit());
            }
        }
    }
}
