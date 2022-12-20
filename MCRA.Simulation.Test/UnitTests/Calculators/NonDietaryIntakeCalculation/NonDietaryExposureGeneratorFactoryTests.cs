using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.NonDietaryIntakeCalculation {
    /// <summary>
    /// NonDietaryIntakeCalculation calculator
    /// </summary>
    [TestClass]
    public class NonDietaryExposureGeneratorFactoryTests {
        /// <summary>
        /// NonDietary exposure generator factory: matchSpecificIndividuals = true, isCorrelationBetweenIndividuals = false
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGeneratorFactory_TestCreateMatched() {
            var settings = new MockNonDietaryExposureGeneratorFactorySettings() { 
                MatchSpecificIndividuals= true,
                IsCorrelationBetweenIndividuals = false
            };
            var nonDietaryExposureGeneratorFactory = new NonDietaryExposureGeneratorFactory(settings);
            var calculator = nonDietaryExposureGeneratorFactory.Create();
            Assert.IsTrue(calculator is NonDietaryMatchedExposureGenerator);
        }
        /// <summary>
        /// NonDietary exposure generator factory: matchSpecificIndividuals = false, isCorrelationBetweenIndividuals = false
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGeneratorFactory_TestCreateUnmatched() {
            var settings = new MockNonDietaryExposureGeneratorFactorySettings() {
                MatchSpecificIndividuals = false,
                IsCorrelationBetweenIndividuals = false
            };
            var nonDietaryExposureGeneratorFactory = new NonDietaryExposureGeneratorFactory(settings);
            var calculator = nonDietaryExposureGeneratorFactory.Create();
            Assert.IsTrue(calculator is NonDietaryUnmatchedExposureGenerator);
        }
        /// <summary>
        /// NonDietary exposure generator factory: matchSpecificIndividuals = false, isCorrelationBetweenIndividuals = true
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGeneratorFactory_TestCreateUnmatchedCorrelated() {
            var settings = new MockNonDietaryExposureGeneratorFactorySettings() {
                MatchSpecificIndividuals = false,
                IsCorrelationBetweenIndividuals = true
            };
            var nonDietaryExposureGeneratorFactory = new NonDietaryExposureGeneratorFactory(settings);
            var calculator = nonDietaryExposureGeneratorFactory.Create();
            Assert.IsTrue(calculator is NonDietaryUnmatchedCorrelatedExposureGenerator);
        }
    }
}
