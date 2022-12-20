using MCRA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class ISUFFrequencyCalculator {

        private readonly int _gridPrecision;
        private readonly int _numberOfIterations;

        public ISUFFrequencyCalculator(
            int gridPrecision,
            int numberOfIterations
        ) {
            _gridPrecision = gridPrecision;
            _numberOfIterations = numberOfIterations;
        }

        /// <summary>
        /// Estimates the exposure frequency distribution. Based on A Technical Guide to C-Side,
        /// Technical Report 96-TR 32, K.W. Dodd, p49-52.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <returns></returns>
        public ISUFFrequencyCalculationResult CalculateDiscreteFrequencyDistribution(
            ICollection<SimpleIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            var groupedIndividualDayIntakes = dietaryIndividualDayIntakes
                .GroupBy(idi => idi.SimulatedIndividualId);

            var intakeFrequencies = groupedIndividualDayIntakes
                .Select(g => new IndividualFrequency() {
                    SimulatedIndividualId = g.Key,
                    Frequency = g.Count(idi => idi.Amount > 0),
                });

            var frequencyLevels= groupedIndividualDayIntakes.Max(c => c.Count()) + 1;

            var intakeFrequencyCount = new List<int>(frequencyLevels);
            for (int i = 0; i < frequencyLevels; i++) {
                intakeFrequencyCount.Add(intakeFrequencies.Count(c => c.Frequency == i));
            }

            var numberOfConsumers = intakeFrequencyCount.Sum();
            var n = intakeFrequencyCount.Count;
            var k = n - 1;
            var m = _gridPrecision + 1;
            var pm = new double[m];
            var theta = new double[m, m];
            var qm = new double[m];
            var Qm = new double[m];
            var L = new double[m];
            var psi = new double[n, m];
            var psiHat = new double[n];
            var psiSlash = new double[n];
            var C = new double[n, m];
            var I = new double[m, m];
            var theta0 = new double[m];

            for (int i = 0; i < m; i++) {
                for (int j = 0; j < m; j++) {
                    if (i == j) {
                        I[i, j] = 1;
                    } else {
                        I[i, j] = 0;
                    }
                }
            }

            var meanPsiHat = 0D;
            for (int i = 0; i < n; i++) {
                psiHat[i] = Convert.ToDouble(intakeFrequencyCount[i]) / Convert.ToDouble(numberOfConsumers);
                meanPsiHat += 1.0 * i / k * intakeFrequencyCount[i];
                for (int j = 0; j < m; j++) {
                    pm[j] = 1.0 * j / _gridPrecision;
                    C[i, j] = Math.Pow(pm[j], i) * Math.Pow((1 - pm[j]), k - i) * nCombination(k, i);
                }
            }

            meanPsiHat = meanPsiHat / numberOfConsumers;
            var oneMinusMeanPsiK = Math.Pow((1 - meanPsiHat), k);
            var psiHat0 = psiHat[0];
            var max0 = Math.Max(psiHat0, oneMinusMeanPsiK);
            psiSlash[0] = max0;
            var meanPsiK = Math.Pow(meanPsiHat, k);
            var psiHatK = psiHat[n - 1];
            var maxK = Math.Max(psiHatK, meanPsiK);
            psiSlash[n - 1] = maxK;
            for (int i = 1; i < k; i++) {
                psiSlash[i] = psiHat[i] * (1 - max0 - maxK) / (1 - psiHat0 - psiHatK);
                if (double.IsNaN(psiSlash[i])) {
                    psiSlash[i] = 0;
                }
            }
            var L_opt = 1e6;
            var i_opt = 0D;
            var productRatio = 0D;
            var totalQm = 0D;
            if (meanPsiHat != 1) {
                for (int i = 0; i < _numberOfIterations * 1000; i++) {
                    var pos = 0;
                    var minL = double.PositiveInfinity;
                    for (int j = 0; j < m; j++) {
                        for (int jj = 0; jj < m; jj++) {
                            if (I[j, jj] == 1) {
                                theta[j, jj] = (qm[jj] + 1) / (i + 1);
                            } else {
                                theta[j, jj] = qm[jj] / (i + 1);
                            }
                        }
                    }

                    for (int j = 0; j < m; j++) {
                        var sumPsiHat = 0D;
                        var sumProductRatio = 0D;
                        for (int p = 0; p < n; p++) {
                            var psiTot = 0D;
                            for (int jj = 0; jj < m; jj++) {
                                psiTot += C[p, jj] * theta[j, jj];
                            }
                            psi[p, j] = psiTot;
                            if (psiSlash[p] != 0) {
                                sumPsiHat += Math.Pow((psiHat[p] - psi[p, j]), 2) / psiSlash[p];
                            }
                        }

                        theta0[j] = theta[j, 0];
                        theta[j, 0] = 0;
                        for (int jj = 0; jj < m; jj++) {
                            if (theta0[j] != 1) {
                                var ratio = theta[j, jj] / (1 - theta0[j]);
                                if (ratio == 0) {
                                    productRatio = 0;
                                } else {
                                    var logRatio = UtilityFunctions.LogBound(ratio);
                                    productRatio = ratio * logRatio;
                                }
                                sumProductRatio += productRatio;
                            } else {
                                sumProductRatio = 0;
                            }
                        }
                        L[j] = numberOfConsumers * sumPsiHat + sumProductRatio;
                        if (minL > L[j]) {
                            minL = L[j];
                            pos = j;
                        }
                    }
                    qm[pos] = qm[pos] + 1;
                    if (minL < L_opt) {
                        for (int j = 0; j < m; j++) {
                            Qm[j] = qm[j];
                            i_opt = i;
                            L_opt = minL;
                        }
                    }
                }

                if (i_opt == 0) {
                    var sumQm = 0D;
                    for (int i = 0; i < m; i++) {
                        sumQm = +Qm[i];
                    }
                    i_opt = sumQm;
                } else {
                    i_opt = 0;
                    for (int i = 0; i < Qm.Length; i++) {
                        i_opt += Qm[i];
                    }
                }
                for (int i = 0; i < m; i++) {
                    Qm[i] = Qm[i] / i_opt;
                    totalQm += Qm[i];
                }
            } else {
                Qm[m - 1] = 1;
            }

            var result = new ISUFFrequencyCalculationResult() {
                PM = pm,
                DiscreteFrequencies = Qm.ToList(),
            };
            return result;
        }

        /// <summary>
        /// Calculates the number of permutations for l objects taken from a set of size k.
        /// </summary>
        /// <param name="k">Size of set</param>
        /// <param name="l">Number of objects</param>
        /// <returns>The number of permutions</returns>
        private long nCombination(int k, int l) {
            long a = 1;
            long b = 1;
            long c = 1;
            for (int i = 1; i < k + 1; i++) {
                a *= i;
            }

            if (l == 0) {
                b = 1;
            } else {
                for (int i = 1; i < l + 1; i++) {
                    b *= i;
                }
            }
            for (int i = 1; i < k - l + 1; i++) {
                c *= i;
            }
            return a / (b * c);
        }
    }
}
