using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MathNet.Numerics.Distributions;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeTransformers;
using MCRA.Utils.Statistics.Modelling;
using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Iowa State University Food Model for chronic exposure assessment
    /// </summary>
    public class ISUFModel : IntakeModel {

        private TransformType _transformType;
        private IISUFModelCalculationSettings _isufModelSettings;
        private readonly int _N400 = 400;

        public ISUFFrequencyCalculationResult FrequencyResult { get; set; }
        public TransformationCalculationResult TransformationResult { get; set; }
        public UsualIntakeResult UsualIntakeResult { get; set; }

        /// <summary>
        /// The exposure model type.
        /// </summary>
        public override IntakeModelType IntakeModelType {
            get {
                return IntakeModelType.ISUF;
            }
        }

        public ISUFModel(
            TransformType transformType,
            IISUFModelCalculationSettings isufModelSettings
        ) {
            _transformType = transformType;
            _isufModelSettings = isufModelSettings;
        }

        public override void CalculateParameters(
            ICollection<SimpleIndividualDayIntake> individualDayIntakeAmounts
        ) {
            var frequencyCalculator = new ISUFFrequencyCalculator(_isufModelSettings.GridPrecision, _isufModelSettings.NumberOfIterations);
            var frequenciesResult = frequencyCalculator
                .CalculateDiscreteFrequencyDistribution(individualDayIntakeAmounts);

            var positiveIndividualDayIntakeAmounts = individualDayIntakeAmounts
                .Where(r => r.Amount > 0)
                .OrderBy(r => r.Amount)
                .ToList();

            var intakeTransformer = IntakeTransformerFactory.Create(
                _transformType,
                () => PowerTransformer.CalculatePower(positiveIndividualDayIntakeAmounts.Select(r => r.Amount))
            );
            _transformType = intakeTransformer.TransformType;

            var transformedPositiveIndividualDayIntakeAmounts = positiveIndividualDayIntakeAmounts
                .Select(r => intakeTransformer.Transform(r.Amount))
                .ToList();

            var transformationCalculator = new ISUFTransformationCalculator(_isufModelSettings.IsSplineFit);
            TransformationResult = transformationCalculator.Transform(transformedPositiveIndividualDayIntakeAmounts);

            var result = calculateVarianceComponents(
                positiveIndividualDayIntakeAmounts,
                TransformationResult.ZHat
            );
            TransformationResult.VarianceBetweenUnit = result.VarianceBetweenUnit;
            TransformationResult.VarianceWithinUnit = result.VarianceWithinUnit;
            TransformationResult.MA4 = result.MA4;
            TransformationResult.PMA4 = result.PMA4;
            TransformationResult.Power = (intakeTransformer as PowerTransformer)?.Power ?? double.NaN;

            // Corrects the usual exposure distribution for heterogeneity within persons between days.
            var xI400 = TransformationResult.Z400
                .Select(c => c * Math.Sqrt(TransformationResult.VarianceBetweenUnit))
                .ToList();

            var yI400 = correctHeterogeneity(
                transformedPositiveIndividualDayIntakeAmounts,
                TransformationResult.Z400,
                TransformationResult.GZ400,
                xI400,
                TransformationResult.NumberOfKnots,
                TransformationResult.VarianceWithinUnit,
                TransformationResult.MA4,
                intakeTransformer
            );
            var cumF200 = TransformationResult.Z400.Select(c => NormalDistribution.CDF(0, 1, c)).ToList();

            UsualIntakeResult = integrateDistributions(
                frequenciesResult.DiscreteFrequencies,
                transformedPositiveIndividualDayIntakeAmounts,
                frequenciesResult.PM,
                TransformationResult.ZHat,
                TransformationResult.Z,
                TransformationResult.GZ,
                yI400,
                xI400,
                cumF200
            );
            FrequencyResult = frequenciesResult;
        }


        public override List<ConditionalUsualIntake> GetConditionalIntakes(
            int seed,
            CompositeProgressState progressState = null
        ) {
            return null;
        }

        public override List<ModelBasedIntakeResult> GetMarginalIntakes(
            int seed,
            CompositeProgressState progressState = null
        ) {
            return null;
        }

        public override List<ModelAssistedIntake> GetIndividualIntakes(int seed) {
            return null;
        }

        /// <summary>
        /// Estimates variance components for between and within individuals.
        /// The used algorithm is Expected Means Squares (EMS).
        /// </summary>
        private TransformationCalculationResult calculateVarianceComponents(
            ICollection<SimpleIndividualDayIntake> positiveIndividualDayIntakes,
            List<double> zHat
        ) {
            var intakeFrequencies = positiveIndividualDayIntakes
                .GroupBy(idi => idi.SimulatedIndividual)
                .Select(g => new IndividualFrequency(g.Key) {
                    Frequency = g.Count(idi => idi.Amount > 0),
                });

            var individualId = positiveIndividualDayIntakes.Select(idi => idi.SimulatedIndividual.Id).ToList();
            var individualIdDistinct = individualId.Distinct().ToList();
            var intakeFrequenciesIndividual = intakeFrequencies.Select(f => f.Frequency).ToList();

            var meanZHat = new double[intakeFrequenciesIndividual.Count];
            var sdWithin = new double[intakeFrequenciesIndividual.Count];
            var ssWithin = new double[intakeFrequenciesIndividual.Count];

            var counter = 0;
            foreach (var item in intakeFrequenciesIndividual) {
                meanZHat[counter] = 0;
                ssWithin[counter] = 0;
                for (int j = 0; j < zHat.Count; j++) {
                    if (individualIdDistinct[counter] == individualId[j]) {
                        meanZHat[counter] += zHat[j];
                    }
                }
                meanZHat[counter] = meanZHat[counter] / item;
                for (int j = 0; j < zHat.Count; j++) {
                    if (individualIdDistinct[counter] == individualId[j]) {
                        ssWithin[counter] += Math.Pow((zHat[j] - meanZHat[counter]), 2);
                    }
                }

                if (item != 1) {
                    ssWithin[counter] = ssWithin[counter] / (item - 1);
                } else {
                    ssWithin[counter] = 0;
                }
                counter++;
            }

            var observedError = MixedModelCalculator.MLRandomModel(zHat, individualId, individualIdDistinct);
            var adResults = StatisticalTests.AndersonDarling(observedError);
            var SSWithin = 0D;
            var k = 0D;
            var tot = 0D;
            var ssNiv = 0D;
            for (int i = 0; i < individualIdDistinct.Count; i++) {
                ssNiv += Math.Pow(meanZHat[i], 2) * intakeFrequenciesIndividual[i];
                SSWithin += ssWithin[i] * (intakeFrequenciesIndividual[i] - 1);
                tot += meanZHat[i] * intakeFrequenciesIndividual[i];
                k += Math.Pow(intakeFrequenciesIndividual[i], 2);
            }

            var ssBetween = ssNiv - Math.Pow(tot, 2) / individualId.Count;
            k /= individualId.Count;
            var varianceWithin = SSWithin / (individualId.Count - intakeFrequenciesIndividual.Count);
            var varianceBetween = (ssBetween - (intakeFrequenciesIndividual.Count - 1) * varianceWithin) / (individualId.Count - k);
            var resultComponents = normalizeVarianceComponents(varianceBetween, varianceWithin);
            var resultSkewness = calculateSkewness(intakeFrequenciesIndividual, ssWithin, resultComponents.VarianceWithinUnit);
            var result = new TransformationCalculationResult() {
                VarianceBetweenUnit = resultComponents.VarianceBetweenUnit,
                VarianceWithinUnit = resultComponents.VarianceWithinUnit,
                MA4 = resultSkewness.MA4,
                PMA4 = resultSkewness.PMA4
            };
            return result;
        }

        /// <summary>
        /// Normalize to unity
        /// </summary>
        private TransformationCalculationResult normalizeVarianceComponents(double varianceBetween, double varianceWithin) {
            double varianceBetweenUnit;
            double varianceWithinUnit;
            if (varianceWithin < 0) {
                varianceWithinUnit = 0.0001;
                varianceBetweenUnit = 1 - varianceWithinUnit;
            } else if (varianceBetween < 0) {
                varianceBetweenUnit = 0.9999;
                varianceWithinUnit = 1 - varianceBetweenUnit;
            } else {
                var sum = varianceBetween + varianceWithin;
                varianceBetweenUnit = varianceBetween / sum;
                varianceWithinUnit = varianceWithin / sum;
            }
            var result = new TransformationCalculationResult() {
                VarianceBetweenUnit = varianceBetweenUnit,
                VarianceWithinUnit = varianceWithinUnit,

            };
            return result;
        }

        /// <summary>
        /// Calculates statistics for heterogeneity and skewness (ISUF).
        /// </summary>
        private TransformationCalculationResult calculateSkewness(List<double> intakeFrequenciesIndividual, double[] ssWithin, double varianceWithin) {
            var sumKi = 0D;
            var sma4 = 0D;
            var count = 0;
            for (int i = 0; i < intakeFrequenciesIndividual.Count; i++) {
                if (intakeFrequenciesIndividual[i] > 1) {
                    var d = Convert.ToDouble(intakeFrequenciesIndividual[i] - 1);
                    sumKi += (1.0 / (1 + 2.0 / d) * Math.Pow(ssWithin[i], 2));
                    var sum1 = (8.0 / d + 40.0 / (d * d) + 48.0 / (d * d * d));
                    var sum2 = Math.Pow((1 + 2 / d), 2);
                    sma4 += sum1 / sum2;
                    count += 1;
                }
            }
            if (varianceWithin == 0) {
                varianceWithin = 0.00001;
            }
            var mA4 = 3.0 / (Math.Pow(varianceWithin, 2)) * sumKi / Convert.ToDouble(count);
            sma4 = 3 * Math.Sqrt(sma4) / Convert.ToDouble(count);
            var tma4 = Math.Abs(mA4 - 3) / sma4;
            var pMA4 = 2 * StudentT.CDF(0, 1, Convert.ToDouble(count - 1), tma4);
            var result = new TransformationCalculationResult() {
                PMA4 = pMA4,
                MA4 = mA4,
                VarianceWithinUnit = varianceWithin
            };
            return result;
        }

        /// <summary>
        /// Corrects the usual exposure distribution for heterogeneity within persons between days.
        /// </summary>
        private List<double> correctHeterogeneity(
                List<double> transformedIndividualIntakes,
                List<double> Z400,
                List<double> GZ400,
                List<double> xI400,
                int numberOfKnots,
                double varianceWithin,
                double ma4,
                IntakeTransformer intakeTransformer
            ) {
            if (_isufModelSettings.IsSplineFit) {
                return ninePointsSplineApproximation(Z400, GZ400, xI400, numberOfKnots, varianceWithin, ma4, intakeTransformer);
            } else {
                var mu = transformedIndividualIntakes.Average();
                var stdev = Math.Sqrt(transformedIndividualIntakes.Variance());
                return ninePointsApproximation(stdev, mu, xI400, varianceWithin, ma4, intakeTransformer);
            }
        }

        /// <summary>
        /// Calculates usual exposure distribution using spline fit.
        /// The heterogeneity correction is applied through the use of the MA4 statistic.
        /// For the value 3, the distribution is standard normal. For each value a discrete
        /// 9-point approximation of the standard normal corresponding to the first 4 moments
        /// is applied, representing the measurements error model (exposure amounts). ISUF assumes
        /// assumption B, that is each exposure day is an unbiased estimator for usual exposure in the
        /// original scale
        /// </summary>
        private List<double> ninePointsSplineApproximation(
                List<double> Z400,
                List<double> GZ400,
                List<double> xI400,
                int numberOfKnots,
                double varianceWithin,
                double ma4,
                IntakeTransformer intakeTransformer
            ) {
            var wj = new double[9];
            var cj = new double[9];
            setNinePointsSet(wj, cj, varianceWithin, ma4);
            var yI400 = new double[_N400];

            //yI400 contains values on the original scale
            for (int i = 0; i < 9; i++) {
                var storeXi400 = new List<double>();
                for (int j = 0; j < _N400; j++) {
                    storeXi400.Add(xI400[j] + cj[i]);
                }
                var storeYi400 = UtilityFunctions.LinearInterpolate(GZ400, Z400, storeXi400);

                for (int j = 0; j < _N400; j++) {
                    storeYi400[j] = intakeTransformer.InverseTransform(storeYi400[j]);
                    if (storeYi400[j] >= 0) {
                        storeYi400[j] = storeYi400[j] * wj[i];
                    } else {
                        storeYi400[j] = 0;
                    }
                    yI400[j] += storeYi400[j];
                }
            }

            var tI400 = yI400.Select(c => intakeTransformer.Transform(c)).ToList();
            var wt = Enumerable.Repeat(1.0, _N400).ToList();
            var knots = new double[numberOfKnots];
            for (int i = 0; i < numberOfKnots; i++) {
                knots[i] = ((i + 1.0) / (numberOfKnots + 1) - .5) * (xI400[_N400 - 1] - xI400[0]);
            }

            var gZ = SplineRegression.MonotoneRegressionSpline(xI400, tI400, wt, knots);
            yI400 = gZ.Select(c => intakeTransformer.InverseTransform(c)).ToArray();
            return yI400.ToList();
        }

        /// <summary>
        /// Estimate usual exposure distribution using the discrete 9-point approximationof the normal error measurement model.
        /// </summary>
        private List<double> ninePointsApproximation(
                double stdev,
                double mu,
                List<double> xI400,
                double varianceWithin,
                double ma4,
                IntakeTransformer intakeTransformer
            ) {
            var wj = new double[9];
            var cj = new double[9];
            setNinePointsSet(wj, cj, varianceWithin, ma4);
            var storeXi400 = new double[_N400];
            var storeYi400 = new double[_N400];
            var yI400 = new double[_N400];

            for (int i = 0; i < 9; i++) {
                for (int j = 0; j < _N400; j++) {
                    storeXi400[j] = xI400[j] + cj[i];
                    storeYi400[j] = intakeTransformer.InverseTransform(storeXi400[j] * stdev + mu);
                    if (storeYi400[j] >= 0) {
                        storeYi400[j] = storeYi400[j] * wj[i];
                    } else {
                        storeYi400[j] = 0;
                    }
                    yI400[j] += storeYi400[j];
                }
            }
            return yI400.ToList();
        }

        private void setNinePointsSet(double[] wj, double[] cj, double varianceWithin, double ma4) {
            const double c = 1.537142184;
            var a = (3.215197658 - Math.Sqrt(c * ma4 - 1.795556375)) / c;
            var b = 7.893253129 - 3.483253128 * a;
            wj[0] = 0.873310 - 0.620820 * a;
            wj[1] = 0.159698 * a;
            wj[2] = wj[1];
            wj[3] = 0.070458 * a;
            wj[4] = wj[3];
            wj[5] = 0.080255 * a;
            wj[6] = wj[5];
            wj[7] = 0.063345;
            wj[8] = wj[7];
            cj[0] = 0;
            cj[1] = 0.5 * varianceWithin;
            cj[2] = -cj[1];
            cj[3] = .8 * varianceWithin;
            cj[4] = -cj[3];
            cj[5] = 1.3 * varianceWithin;
            cj[6] = -cj[5];
            cj[7] = Math.Sqrt(b) * varianceWithin;
            cj[8] = -cj[7];
        }

        /// <summary>
        /// Estimation of the usual exposure distribution and percentiles. ISUF only (17-08-2010)
        /// </summary>
        private UsualIntakeResult integrateDistributions(
                List<double> discreteFrequencies,
                List<double> transformedIndividualIntakes,
                double[] pm,
                List<double> zHat,
                List<double> z,
                List<double> gZ,
                List<double> yI400,
                List<double> xI400,
                List<double> cumF200
            ) {
            var theta0 = discreteFrequencies[0];
            var consumersOnly = new double[_N400];
            var usualIntakes = new double[_N400];
            //increase speed by determining the maximum
            var sumT = yI400.Select(c => c * 2).Sum();
            for (int i = 1; i < _N400; i++) {
                consumersOnly[i] = getUsualIntake(cumF200[i], sumT, i, true, discreteFrequencies, pm, yI400, cumF200, consumersOnly, usualIntakes);
                usualIntakes[i] = getUsualIntake(cumF200[i], sumT, i, false, discreteFrequencies, pm, yI400, cumF200, consumersOnly, usualIntakes);
            }

            //correct for wrong values, see explanation bisection algorithm
            //or restore zeros
            for (int i = usualIntakes.Length - 3; i >= 0; i--) {
                if (usualIntakes[i] == 0) {
                    usualIntakes[i] = specifyExtraPolate(i + 2, i, cumF200, usualIntakes);
                }
                if (consumersOnly[i] == 0) {
                    consumersOnly[i] = specifyExtraPolate(i + 2, i, cumF200, consumersOnly);
                }
            }
            var result = new UsualIntakeResult() {
                UsualIntakes = usualIntakes.Zip(xI400, cumF200, (ui, dev, cp) => new ISUFUsualIntake() { UsualIntake = ui, Deviate = dev, CumulativeProbability = cp }).ToList(),
                Diagnostics = zHat.Zip(z, gZ, transformedIndividualIntakes, (x, y, gz, t) => new IsufModelDiagnostics() { Zhat = x, Z = y, GZ = gz, TransformedDailyIntakes = t }).ToList(),
                ConsumersOnly = consumersOnly.ToList(),
            };
            return result;
        }

        /// <summary>
        /// Estimates the usual exposure distribution for a set of 400 points, using the bi-section algorithm.
        /// </summary>
        /// <param name="perc"></param>
        /// <param name="boolConsumersOnly"></param>
        /// <returns></returns>
        private double getUsualIntake(
                double perc,
                double max,
                int index,
                bool boolConsumersOnly,
                List<double> discreteFrequencies,
                double[] pm,
                List<double> yI400,
                List<double> cumF200,
                double[] consumersOnly,
                double[] usualIntakes
            ) {
            var umin = 0D;
            var umax = max;
            var u = 0D;
            for (int i = 0; i < 100; i++) {
                u = (umin + umax) / 2;
                if ((umax - umin) / u < 0.00001) {
                    break;
                }
                var u1 = umin;
                var u2 = u;
                var p1 = probability(u1, perc, boolConsumersOnly, discreteFrequencies, pm, yI400, cumF200);
                var p2 = probability(u2, perc, boolConsumersOnly, discreteFrequencies, pm, yI400, cumF200);
                var ratio = p1 / p2;
                //Dit gaat soms fout, omdat er geen tekenwisseling plaats. p1 is bij het slecht schatten van theta altijd positief
                //waardoor umin = u, en de schatting naar oneindig loopt.
                //Dit komt omdat y discreet is en niet een continue functie

                if (ratio <= 0) {
                    umax = u;
                } else {
                    umin = u;
                }
            }
            if (u >= .5 * max && u <= max) {
                u = extraPolate(boolConsumersOnly, index, cumF200, consumersOnly, usualIntakes);
            }
            return u;
        }

        /// <summary>
        /// Bi-section algorithm, used for estimation of the usual daily exposure distribution.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="perc"></param>
        /// <param name="boolConsumersOnly"></param>
        /// <returns></returns>
        private double probability(
                double u,
                double perc,
                bool boolConsumersOnly,
                List<double> discreteFrequencies,
                double[] pm,
                List<double> yI400,
                List<double> cumF200
            ) {
            var f = 0D;
            for (int j = 1; j < pm.Length; j++) {
                var position = 0;
                for (int k = 0; k < yI400.Count; k++) {
                    if (u / pm[j] <= yI400[k]) {
                        position = k;
                        break;
                    }
                    if (k == yI400.Count - 1) {
                        position = k;
                    }
                }
                //yI400 together with cumF200 determine the accuracy of the percentiles
                f += cumF200[position] * discreteFrequencies[j];
            }

            if (boolConsumersOnly == true) {
                f = f / (1 - discreteFrequencies[0]) - perc;
            } else {
                f = f + discreteFrequencies[0] - perc;
            }
            return f;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cOnly"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private double extraPolate(bool cOnly, int index, List<double> cumF200, double[] consumersOnly, double[] usualIntakes) {
            var y = new List<double>(2);
            var u = 0D;
            if (index > 1) {
                if (cOnly) {
                    y.Add(consumersOnly[index - 2]);
                    y.Add(consumersOnly[index - 1]);
                } else {
                    y.Add(usualIntakes[index - 2]);
                    y.Add(usualIntakes[index - 1]);
                }
                var x = new List<double> { cumF200[index - 2], cumF200[index - 1] };
                var slrResult = SimpleLinearRegressionCalculator.Compute(x, y);
                u = slrResult.Constant + slrResult.Coefficient * cumF200[index];
            }
            return u;
        }

        private double specifyExtraPolate(int index, int arrayIndex, List<double> cumF200, double[] yVal) {
            var y = new List<double> { yVal[index - 1], yVal[index] };
            var x = new List<double> { cumF200[index - 1], cumF200[index] };
            var slrResult = SimpleLinearRegressionCalculator.Compute(x, y);
            var u = slrResult.Constant + slrResult.Coefficient * cumF200[arrayIndex];
            if (u < 0) {
                u = 0;
            }
            return u;
        }
    }
}
