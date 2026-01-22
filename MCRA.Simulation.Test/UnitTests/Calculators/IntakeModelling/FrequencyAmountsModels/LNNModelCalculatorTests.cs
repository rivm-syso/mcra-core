using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class LNNModelCalculatorTests {

        /// <summary>
        /// Unit test not implemented
        /// </summary>
        [TestMethod]
        [DataRow(0.5, -0.5, 1, 1)]
        [DataRow(0.5, 0, 1, 1)]
        [DataRow(0.5, 0.5, 1, 1)]
        public void LNNModelCalculator_TestCompute(
            double pMedian,
            double correlation,
            double sigmaB,
            double sigmaW
        ) {
            var seed = 1;
            var numSubjects = 10000;
            var nDays = 2;

            var gmAmounts = 100;
            var sigmaLogit = 1;

            // Create frequency design matrix
            var freqDesign = new double[numSubjects * nDays, 1];
            for (int i = 0; i < numSubjects * nDays; i++) {
                freqDesign[i, 0] = 1;
            }

            // Create frequency design matrix
            var amountsDesign = new double[numSubjects * nDays, 1];
            for (int i = 0; i < numSubjects * nDays; i++) {
                amountsDesign[i, 0] = 1;
            }

            var transformer = new LogTransformer();
            var init = new LNNParameters() {
                FreqEstimates = [0.5D],
                AmountEstimates = [4D],
                Parameters = new LNNParameterEstimate() {
                    Dispersion = sigmaLogit,
                    Correlation = .3,
                    Power = 0D,
                    VarianceBetween = 1.1D,
                    VarianceWithin = .7D
                },
            };

            var amounts = simulate(
                numIndividuals: numSubjects,
                nDays: nDays,
                pMedian: pMedian,
                gmAmounts: gmAmounts,
                sigmaB: sigmaB,
                sigmaW: sigmaW,
                sigmaLogit: sigmaLogit,
                rho: correlation,
                seed: seed
            );

            var weights = Enumerable.Repeat(1D, numSubjects * nDays).ToList();
            var frequencies = Enumerable.Repeat(nDays, numSubjects).ToList();
            var calculator = new LNNModelCalculator();
            var result = calculator.ComputeFit(
                amounts,
                weights,
                frequencies,
                freqDesign,
                amountsDesign,
                transformer,
                init
            );

            var expectedMuAmount = Math.Log(gmAmounts);
            var expectedMuFreq = UtilityFunctions.Logit(pMedian);
            var expectedDispersion = sigmaLogit * sigmaLogit;
            Assert.AreEqual(expectedMuFreq, result.Parameters.FreqEstimates[0], 1e-1);
            Assert.AreEqual(expectedMuAmount, result.Parameters.AmountEstimates[0], 1e-1);
            Assert.AreEqual(correlation, result.Parameters.Parameters.Correlation, .5);
            Assert.AreEqual(expectedDispersion, result.Parameters.Parameters.Dispersion, 1e-1);
            Assert.AreEqual(sigmaB * sigmaB, result.Parameters.Parameters.VarianceBetween, 2e-1);
            Assert.AreEqual(sigmaW * sigmaW, result.Parameters.Parameters.VarianceWithin, 1e-1);
        }

        /// <summary>
        /// Generates consumption amounts based on a LNN model for the specified
        /// number of individuals, days, and parameters.
        /// </summary>
        /// <param name="numIndividuals">Number of individuals.</param>
        /// <param name="nDays">Days per individual.</param>
        /// <param name="pMedian">Median daily consumption probability.</param>
        /// <param name="gmAmounts">Geometric mean of amounts distribution.</param>
        /// <param name="sigmaB">Between-person SD on log amount (log-scale).</param>
        /// <param name="sigmaW">Within-person  SD on log amount (log-scale).</param>
        /// <param name="sigmaLogit">SD of person random effect in frequency.</param>
        /// <param name="rho">Corr(a_i, b_i) between person-level frequency and amount effects.</param>
        /// <param name="xi">Correlation within person between day-level consumption and intake.</param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public List<double> simulate(
            int numIndividuals,
            int nDays = 2,
            double pMedian = 0.5,
            double gmAmounts = 100,
            double sigmaB = 1,
            double sigmaW = 1,
            double sigmaLogit = 1,
            double rho = 0,
            double xi = 0,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);

            // Amount parameters: set so GM = exp(mu)
            var mu = Math.Log(gmAmounts);

            // Covariance matrix for frequency and amounts for between-person effects
            var VI = new double[,] {
                { sigmaLogit * sigmaLogit, rho * sigmaLogit * sigmaB },
                { rho * sigmaLogit * sigmaB, sigmaB * sigmaB }
            };

            // Person-specific consumption probabilities
            var alpha = UtilityFunctions.Logit(pMedian);
            var distributionIndividualAmounts = new MultiVariateNormalDistribution([alpha, mu], VI);
            var individualFrequenciesAmounts = distributionIndividualAmounts.Samples(random, numIndividuals);

            // Individual consumption probabilities
            var pi = new double[numIndividuals];
            for (int i = 0; i < numIndividuals; i++) {
                pi[i] = UtilityFunctions.ILogit(individualFrequenciesAmounts[i][0]);
            }

            // Covariance matrix for amounts for within-person day effects
            var VD = new double[,] {
                { 1, xi * sigmaW },
                { xi * sigmaW, sigmaW * sigmaW }
            };

            // Realisations
            var distributionDays = new MultiVariateNormalDistribution([0, 0], VD);
            var individualDaysConsumptionSamples = distributionDays.Samples(random, numIndividuals * nDays);
            var z1 = pi.Select(p => NormalDistribution.InvCDF(0, 1, p)).ToArray();
            var consume = Enumerable.Range(0, numIndividuals * nDays)
                .Select(i => individualDaysConsumptionSamples[i][0] < z1[i / nDays] ? 1D : 0D)
                .ToArray();

            // Positive amounts on log scale (only when consumed)
            var amounts = Enumerable.Range(0, numIndividuals * nDays)
                .Select(i => individualFrequenciesAmounts[i / nDays][1] + individualDaysConsumptionSamples[i][1])
                .Select((v, i) => consume[i] == 1 ? Math.Exp(v) : 0)
                .ToArray();

            return amounts.ToList();
        }
    }
}
