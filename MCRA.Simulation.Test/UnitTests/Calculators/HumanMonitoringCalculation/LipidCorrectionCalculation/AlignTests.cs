using MCRA.General;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.LipidCorrectionCalculation {
    [TestClass]
    public class AlignTests {
        /// <summary>
        /// Concentration unit test
        /// </summary>
        /// <param name="targetConcentrationUnit"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="result"></param>
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerdL, 100)]
        [DataRow(ConcentrationUnit.ugPerL, ConcentrationUnit.mgPerL, 1000)]
        [DataRow(ConcentrationUnit.mgPerL, ConcentrationUnit.mgPerdL, 100)]
        [TestMethod]
        public void StandardisationAlign_TestCreate(
                ConcentrationUnit targetConcentrationUnit,
                ConcentrationUnit concentrationUnit,
                double result
            ) {
            var massUnit = concentrationUnit.GetConcentrationMassUnit();
            var targetMassUnit = targetConcentrationUnit.GetConcentrationMassUnit();
            var amountUnit = concentrationUnit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(targetMassUnit);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Grams, 1);
            var multiplier = multiplier1 / multiplier2;
            Assert.AreEqual(result, multiplier);
        }
    }
}
