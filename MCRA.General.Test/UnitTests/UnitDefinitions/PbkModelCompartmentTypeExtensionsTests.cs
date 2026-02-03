namespace MCRA.General.Test.UnitTests.UnitDefinitions {
    [TestClass]
    public class PbkModelCompartmentTypeExtensionsTests {

        [TestMethod]
        [DataRow(PbkModelCompartmentType.AlveolarAir, BiologicalMatrix.Undefined)]
        [DataRow(PbkModelCompartmentType.Liver, BiologicalMatrix.Liver)]
        [DataRow(PbkModelCompartmentType.Fat, BiologicalMatrix.BodyFat)]
        public void PbkModelCompartmentType_TestGetBiologicalMatrix(
            PbkModelCompartmentType type,
            BiologicalMatrix biologicalMatrix
        ) {
            var result = type.GetBiologicalMatrix();
            Assert.AreEqual(biologicalMatrix, result);
        }

        /// <summary>
        /// Check whether each compartment type is linked to a biological
        /// matrix. Only allow some exceptions from a tabu list.
        /// </summary>
        [TestMethod]
        public void PbkModelCompartmentType_TestCompleteness() {
            var tabuList = new HashSet<PbkModelCompartmentType>() {
                PbkModelCompartmentType.Undefined,
                PbkModelCompartmentType.Other,
                PbkModelCompartmentType.AlveolarAir,
                PbkModelCompartmentType.Gut,
                PbkModelCompartmentType.ExposedSkin,
                PbkModelCompartmentType.UnexposedSkin,
                PbkModelCompartmentType.RestOfBody
            };
            var enumValues = Enum
                .GetValues(typeof(PbkModelCompartmentType))
                .Cast<PbkModelCompartmentType>()
                .ToList();
            foreach (var t in enumValues) {
                var result = t.GetBiologicalMatrix();
                if (!tabuList.Contains(t)) {
                    Assert.AreNotEqual(BiologicalMatrix.Undefined, result);
                } else {
                    Assert.AreEqual(BiologicalMatrix.Undefined, result);
                }
            }
        }
    }
}
