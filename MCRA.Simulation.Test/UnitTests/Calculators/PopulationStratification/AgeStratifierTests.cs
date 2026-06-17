using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PopulationStratification {

    [TestClass]
    public class AgeStratifierTests {

        [TestMethod]
        [DataRow(15d,"<= 18")]
        [DataRow(18d,"18 - 50")]
        [DataRow(20d,"18 - 50")]
        [DataRow(50d,"50 - 65")]
        [DataRow(60d,"50 - 65")]
        [DataRow(65d,"> 65")]
        [DataRow(80d,"> 65")]
        public void AgeStratifier_TestLevelName(
            double age,
            string expected
        ) {
            var ageBoundaries = new double[] { 18, 50, 65 };
            var individual = FakeIndividualsGenerator.CreateSingle(25, 2, age: age);
            var stratifier = new AgeStratifier(ageBoundaries);
            var level = stratifier.GetLevel(individual);
            Assert.AreEqual(expected, level.Name);
        }

        [TestMethod]
        [DataRow(15d, "le18")]
        [DataRow(18d, "18-50")]
        [DataRow(20d, "18-50")]
        [DataRow(50d, "50-65")]
        [DataRow(60d, "50-65")]
        [DataRow(65d, "gt65")]
        [DataRow(80d, "gt65")]
        public void AgeStratifier_TestLevelCode(
            double age,
            string expected
        ) {
            var ageBoundaries = new double[] { 18, 50, 65 };
            var individual = FakeIndividualsGenerator.CreateSingle(25, 2, age: age);
            var stratifier = new AgeStratifier(ageBoundaries);
            var level = stratifier.GetLevel(individual);
            Assert.AreEqual(expected, level.Code);
        }
    }
}
