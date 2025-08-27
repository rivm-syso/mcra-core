namespace MCRA.General.Test.UnitTests.UnitDefinitions {
    [TestClass]
    public class PointOfDepartureTypeExtensionsTests {

        /// <summary>
        /// Test whether there is a potency origin type for each hazard characterisation
        /// type.
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationType_TestToPotencyOrigin() {
            var enumValues = Enum
                .GetValues(typeof(HazardCharacterisationType))
                .Cast<HazardCharacterisationType>()
                .Where(r => r != HazardCharacterisationType.Unspecified)
                .Where(r => r != HazardCharacterisationType.Other)
                .ToList();

            // Check whether there is potency origin for each hazard characterisation type.
            foreach (var value in enumValues) {
                var potencyOrigin = value.ToPotencyOrigin();
                Assert.AreNotEqual(
                    PotencyOrigin.Unknown,
                    potencyOrigin,
                    $"No potency origin for hazard characterisation type {value}."
                );
            }
        }

        /// <summary>
        /// Test whether there is a potency origin type for each point of departure type.
        /// </summary>
        [TestMethod]
        public void PointOfDepartureType_TestToPotencyOrigin() {
            var enumValues = Enum
                .GetValues(typeof(PointOfDepartureType))
                .Cast<PointOfDepartureType>()
                .Where(r => r != PointOfDepartureType.Unspecified)
                .ToList();

            // Check whether there is potency origin for each hazard characterisation type.
            foreach (var value in enumValues) {
                var potencyOrigin = value.ToPotencyOrigin();
                Assert.AreNotEqual(
                    PotencyOrigin.Unknown,
                    potencyOrigin,
                    $"No potency origin for point of departure type {value}."
                );
            }
        }
    }
}
