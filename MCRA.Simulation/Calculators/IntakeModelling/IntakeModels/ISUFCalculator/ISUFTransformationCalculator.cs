using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class ISUFTransformationCalculator {

        private readonly bool _isSplineFit;
        private readonly int _N400 = 400;

        public ISUFTransformationCalculator(bool isSplineFit) {
            _isSplineFit = isSplineFit;
        }

        public TransformationCalculationResult Transform(
            List<double> transformedIndividualIntakes
        ) {
            var result = new TransformationCalculationResult() {
                Z = Stats.Blom(1.0448, transformedIndividualIntakes.Count),
                Z400 = Stats.Blom(1.0448, _N400),
                GZ = Enumerable.Repeat(double.NaN, transformedIndividualIntakes.Count).ToList(),
            };
            if (_isSplineFit) {
                result = splineTransformation(transformedIndividualIntakes, result.Z, result.Z400);
            } else {
                var mu = transformedIndividualIntakes.Average();
                var stdev = Math.Sqrt(transformedIndividualIntakes.Variance());
                result.ZHat = transformedIndividualIntakes.Select(c => (c - mu) / stdev).ToList();
            }
            result.AndersonDarlingResults = StatisticalTests.AndersonDarling(result.ZHat);
            return result;
        }

        /// <summary>
        /// Knots are determined by the user. 
        /// </summary>
        private TransformationCalculationResult splineTransformation(
            List<double> transformedIndividualIntakes,
            List<double> z,
            List<double> Z400
        ) {
            var zExt = new List<double>();
            var wtExt = new List<double>();
            var tyExt = new List<double>();
            var counter = 0;
            foreach (var item in transformedIndividualIntakes) {
                zExt.Add(z[counter]);
                wtExt.Add(1);
                tyExt.Add(item);
                counter++;
            }
            foreach (var item in Z400) {
                zExt.Add(item);
                wtExt.Add(0);
                tyExt.Add(-9999);
            }
            var combined = zExt.Zip(wtExt, (first, second) => (first, second));
            var ordered = combined.OrderBy(c => c.first);
            var zExtended = ordered.Select(c => c.first).ToList();
            var wtExtended = ordered.Select(c => c.second).ToList();
            combined = zExt.Zip(tyExt, (first, second) => (first, second));
            ordered = combined.OrderBy(c => c.first);
            var tyExtended = ordered.Select(c => c.second).ToList();

            //Calculate set of 400 Blom-scores and the corresponding fitted spline values, see Dodd
            //Determine number of knots according to Anderson-Darling criterium/
            //Parameter isuf indicates if estimates for knots are available (isuf != null)
            //If available, this number is taken to calculate the final analysis.
            var nKnots = new List<int>();
            for (int i = 0; i < 50; i++) {
                nKnots.Add(i + 4);
            }

            var ADValues = new List<double>();
            List<double> zHat = null;
            List<double> gZ = null;
            List<double> GZ400 = null;
            var numberOfKnots = 0;
            foreach (var k in nKnots) {
                gZ = [];
                GZ400 = [];
                var knots = equidistantKnots(k, transformedIndividualIntakes.Count);
                var gZExt = SplineRegression.MonotoneRegressionSpline(zExtended, tyExtended, wtExtended, knots);
                for (int ii = 0; ii < zExtended.Count; ii++) {
                    if (wtExtended[ii] == 1) {
                        gZ.Add(gZExt[ii]);
                    } else {
                        GZ400.Add(gZExt[ii]);
                    }
                }
                zHat = UtilityFunctions.LinearInterpolate(z, gZ, transformedIndividualIntakes);
                var adResults = StatisticalTests.AndersonDarling(zHat);
                ADValues.Add(adResults.ADStatistic);
                if (adResults.ADStatistic < 0.58) {
                    numberOfKnots = k;
                    break;
                } else {
                    numberOfKnots = 0;
                }
            }

            //Determine number of knots when Anderson-Darling criterium is not met.
            var minAD = 1e8D;
            if (numberOfKnots == 0) {
                gZ = [];
                GZ400 = [];

                for (int i = 0; i < ADValues.Count; i++) {
                    if (minAD > ADValues[i]) {
                        minAD = ADValues[i];
                        numberOfKnots = nKnots[i];
                    }
                }
                var knots = equidistantKnots(numberOfKnots, transformedIndividualIntakes.Count);
                var gZExt = SplineRegression.MonotoneRegressionSpline(zExtended, tyExtended, wtExtended, knots);
                for (int ii = 0; ii < zExtended.Count; ii++) {
                    if (wtExtended[ii] == 1) {
                        gZ.Add(gZExt[ii]);
                    } else {
                        GZ400.Add(gZExt[ii]);
                    }
                }
                zHat = UtilityFunctions.LinearInterpolate(z, gZ, transformedIndividualIntakes);
            }
            var result = new TransformationCalculationResult() {
                ZHat = zHat,
                GZ400 = GZ400,
                GZ = gZ,
                Z = z,
                Z400 = Z400,
                NumberOfKnots = numberOfKnots,
            };
            return result;
        }

        /// <summary>
        /// Calculates equidistant knots.
        /// </summary>
        /// <param name="k">number of knots</param>
        /// <param name="n">numer of values of independent variable</param>
        /// <returns></returns>
        private double[] equidistantKnots(int k, int n) {
            var ku = NormalDistribution.InvCDF(0, 1, (n - 2.5) / n);
            var kl = NormalDistribution.InvCDF(0, 1, 2.5 / n);
            var knots = new double[k];
            for (int i = 0; i < k - 2; i++) {
                knots[i + 1] = ((i + 1.0) / (k - 1) - .5) * (ku - kl);
            }
            knots[0] = kl;
            knots[k - 1] = ku;
            return knots;
        }
    }
}
