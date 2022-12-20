using MCRA.Utils.Statistics.Modelling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Utils.Test.UnitTests.Statistics.Modelling {
    [TestClass]
    public class MixedModelCalculatorTests {

        [TestMethod]
        [ExpectedException(typeof(ParameterFitException))]
        public void MixedModelCalculator_TestFitMLRandomModel_Fail() {
            var yR = new List<double>();
            var indR = new List<int>();
            var distinctLevels = new List<int>();
            _ = MixedModelCalculator.MLRandomModel(yR, indR, distinctLevels);
        }

        [TestMethod]
        public void MixedModelCalculator_TestFixMixedModel() {
            var responses = new List<double>() { -1.3, 0.97, -0.41, -1.11, -2.37, 0.23, 1.67, -0.53 };
            var design = new double[,] {
                { 0 },
                { 1 },
                { 0 },
                { 1 },
                { 0 },
                { 1 },
                { 0 },
                { 1 },
            };
            var individuals = new List<int>() { 1, 2, 3, 4, 1, 2, 3, 4 };
            var weights = new List<double>() { 1, 1, 1, 1, 1, 1, 1, 1 };
            var reml = MixedModelCalculator.FitMixedModel(design, responses, individuals, weights, MixedModelMethod.REML);
            Assert.AreEqual(1.626, reml.VarianceBetween, 1e-3);
        }
    }
}
