using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class UtilityFunctionsTests {

        [TestMethod]
        public void BisectionTest1() {
            var nominal = 0.36;
            var nominalUncertaintyUpper = 0.37;
            var upper = 0.6;
            var upperUncertaintyUpper = .75;

            var MAcc = 0D;
            var UAcc = 0D;
            var MUAcc = 0D;
            var UUAcc = 0D;

            var logit = false;
            if (logit) {
                MAcc = UtilityFunctions.Logit(nominal);
                UAcc = UtilityFunctions.Logit(upper);
                MUAcc = UtilityFunctions.Logit(nominalUncertaintyUpper);
                UUAcc = UtilityFunctions.Logit(upperUncertaintyUpper);
            } else {
                MAcc = UtilityFunctions.LogBound(nominal);
                UAcc = UtilityFunctions.LogBound(upper);
                MUAcc = UtilityFunctions.LogBound(nominalUncertaintyUpper);
                UUAcc = UtilityFunctions.LogBound(upperUncertaintyUpper);
            }
            var SAcc = (UAcc - MAcc) / 1.645;
            var SAccAcc = (MUAcc - MAcc) / 1.645;
            var xOpt = StatisticalTests.BiSectionSearch(df => (getSampleVariance(MAcc, SAcc, SAccAcc, df, 1).Percentile(95) - UUAcc), .1, 1000, 100, 0.001);
            var fOpt = getSampleVariance(MAcc, SAcc, SAccAcc, xOpt, 1).Percentile(95);
            Assert.AreEqual(7, Math.Ceiling(xOpt));
        }

        [TestMethod]
        public void BisectionTest2() {
            var percentage = 0.99;
            var nConsumptions = 1000D;
            var nPosConsumptions = 200D;
            var meanConsumptions = 1D;
            var sigmaConsumptions = 0.5;
            var nResidues = 100D;
            var nPosResidues = 2D;
            var meanResidues = 6D;
            var sigmaResidues = 2D;
            var lor = -2D;
            var pPosConsumptions = nPosConsumptions / nConsumptions;
            var pPosResidues = nPosResidues / nResidues;
            var pCorrected = (percentage - (1 - pPosConsumptions)) / pPosConsumptions;

            //censored values
            var pCensoredValue = pPosConsumptions * (1 - pPosResidues);
            var meanCensoredValue = meanConsumptions + lor;
            var sigmaCensoredValue = sigmaConsumptions;

            //detect
            var pDetect = pPosConsumptions * pPosResidues;
            var meanDetect = meanConsumptions + meanResidues;
            var sigmaDetect = Math.Sqrt(Math.Pow(sigmaConsumptions, 2) + Math.Pow(sigmaResidues, 2));

            //bisection search
            var lower = meanCensoredValue - 4 * sigmaCensoredValue;
            var upper = meanDetect + NormalDistribution.InvCDF(0, 1, pCorrected) * sigmaDetect;
            var logExposure = StatisticalTests.BiSectionSearch(limit => getFunction(limit, pPosResidues, meanCensoredValue, sigmaCensoredValue, meanDetect, sigmaDetect), lower, upper, 100, pCorrected, 0.000000001);
            var fCensoredValue = pCensoredValue * NormalDistribution.PDF(meanCensoredValue, sigmaCensoredValue, logExposure);
            var fDetect = pDetect * NormalDistribution.PDF(meanDetect, sigmaDetect, logExposure);
            var wCensoredValue = fCensoredValue / (fCensoredValue + fDetect);
            var wDetect = fDetect / (fCensoredValue + fDetect);
            var exposure = Math.Exp(logExposure);
        }

        private List<double> getSampleVariance(double mean, double sAcc, double sAccAcc, double df, int seed) {
            var r = new List<double>();
            var random = new McraRandomGenerator(seed);
            for (int i = 0; i < 10000; i++) {
                var m = NormalDistribution.Draw(random, mean, sAccAcc);
                var s = sAcc * Math.Sqrt(ChiSquaredDistribution.Draw(random, df) / df);
                r.Add(m + 1.645 * s);
            }
            return r;
        }

        private double getFunction(double z, double fraction, double mean1, double sigma1, double mean2, double sigma2) {
            var x = (1 - fraction) * NormalDistribution.CDF(0, 1, (z - mean1) / sigma1) + fraction * NormalDistribution.CDF(0, 1, (z - mean2) / sigma2);
            return x;
        }
    }
}
