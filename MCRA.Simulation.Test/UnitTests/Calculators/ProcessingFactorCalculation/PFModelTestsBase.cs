using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {


    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public abstract class PFModelTestsBase {

        protected static ProcessingFactor mockProcessingFactor(
            ProcessingDistributionType distributionType,
            double nominal,
            double upper,
            double nominalUncertaintyUpper = double.NaN,
            double upperUncertaintyUpper = double.NaN
        ) {
            var processingType = FakeProcessingTypesGenerator
                .CreateSingle("XXX", distributionType: distributionType);
            var pf = new ProcessingFactor() {
                ProcessingType = processingType,
                Nominal = nominal,
                Upper = upper,
                NominalUncertaintyUpper = nominalUncertaintyUpper,
                UpperUncertaintyUpper = upperUncertaintyUpper
            };
            return pf;
        }

        protected static List<double> drawVariability(
            ProcessingFactorModel model,
            McraRandomGenerator random,
            int n
        ) {
            var samples = new List<double>(n);
            for (int i = 0; i < n; i++) {
                var draw = model.DrawFromDistribution(random);
                samples.Add(draw);
            }
            return samples;
        }

        protected static List<double> runUncertains(
            ProcessingFactorModel model,
            McraRandomGenerator random,
            int n
        ) {
            var samples = new List<double>(n);
            for (int i = 0; i < n; i++) {
                model.Resample(random);
                var draw = model.DrawFromDistribution(random);
                samples.Add(draw);
            }
            Assert.IsTrue(model.IsUncertaintySample());
            return samples;
        }
    }
}
