using MCRA.General;
using MCRA.Simulation.Calculators.PopulationAlignmentCalculation.NonDietaryExposureGenerators;
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
            var calculator = NonDietaryExposureGeneratorFactory.Create(PopulationAlignmentMethod.MatchIndividualID, false);
            Assert.IsTrue(calculator is NonDietaryMatchedExposureGenerator);
        }

        ///// <summary>
        ///// NonDietary exposure generator factory: matchSpecificIndividuals = false, isCorrelationBetweenIndividuals = false
        ///// </summary>
        //[TestMethod]
        //public void NonDietaryExposureGeneratorFactory_TestCreateUnmatched() {
        //    var calculator = NonDietaryExposureGeneratorFactory.Create(PopulationAlignmentMethod.MatchRandom, false);
        //}

        /// <summary>
        /// NonDietary exposure generator factory: matchSpecificIndividuals = false, isCorrelationBetweenIndividuals = false
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGeneratorFactory_TestCreateUnmatchedUnCorrelated() {
            var calculator = NonDietaryExposureGeneratorFactory.Create(PopulationAlignmentMethod.MatchCofactors, false);
            Assert.IsTrue(calculator is NonDietaryUnmatchedExposureGenerator);
        }

        /// <summary>
        /// NonDietary exposure generator factory: matchSpecificIndividuals = false, isCorrelationBetweenIndividuals = true
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGeneratorFactory_TestCreateUnmatchedCorrelated() {
            var calculator = NonDietaryExposureGeneratorFactory.Create(PopulationAlignmentMethod.MatchCofactors, true);
            Assert.IsTrue(calculator is NonDietaryUnmatchedCorrelatedExposureGenerator);
        }
    }
}
