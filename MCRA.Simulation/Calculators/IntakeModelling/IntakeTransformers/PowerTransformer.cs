using MCRA.Utils;
using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Transforms a set of positive daily exposures to normality based on a power (BoxCox) transformation.
    /// </summary>
    public class PowerTransformer : IntakeTransformer {

        public override TransformType TransformType => TransformType.Power;

        public double Power { get; set; }

        public double[,] GaussHermitePoints { get; set; }

        public PowerTransformer() { }

        public PowerTransformer(double power, double[,] gaussHermitePoints) {
            Power = power;
            GaussHermitePoints = gaussHermitePoints;
        }

        public override double Transform(double x) {
            return UtilityFunctions.BoxCox(x, Power);
        }

        public override double InverseTransform(double x) {
            return UtilityFunctions.InverseBoxCox(x, Power);
        }

        /// <summary>
        /// Backtransforms a BLUP (on the linear scale) to the original scale based on a power
        /// transformation (Gauss Hermite Quadrature).
        /// </summary>
        public override double BiasCorrectedInverseTransform(double x, double varianceWithin) {
            return UtilityFunctions.BackTransformAmountBoxCox(x, varianceWithin, Power, GaussHermitePoints);
        }

        /// <summary>
        /// Golden section algorithm,
        /// Charles D. Coleman, NESUG 17 Analysis, p 1-4
        /// </summary>
        /// <param name="y"></param>
        public static double CalculatePower(IEnumerable<double> y) {
            var power = double.NaN;
            var _y = y.Where(i => i > 0).Order().ToList();
            var tol = 1e-7;
            var r = (Math.Sqrt(5) - 1) / 2;
            var a = 1e-4;
            var b = 2D;
            var c = 0D;
            var d = 0D;
            var fa = 0d;
            var fb = 0d;
            var fc = 0d;
            var fd = 0d;
            var sumLog = _y.Sum(i => Math.Log(i));

            for (int ii = 0; ii < 100; ii++) {
                //initialize
                fa = loglik(_y, a, sumLog);
                fb = loglik(_y, b, sumLog);
                c = a + r * (b - a);
                fc = loglik(_y, c, sumLog);
                if (fc < fa || fc < fb) {
                    if (fc < fa) {
                        a -= 2;
                        fa = loglik(_y, a, sumLog);
                    } else {
                        b += 2;
                        fb = loglik(_y, b, sumLog);
                    }
                    c = a + r * (b - a);
                    fc = loglik(_y, c, sumLog);
                } else {
                    break;
                }
            }
            var cdflag = true;
            for (int i = 0; i < 1000; i++) {
                if (b - a > tol) {
                    var diff = r * (b - a);
                    if (!cdflag) {
                        c = a + diff;
                        fc = loglik(_y, c, sumLog);
                    }
                    if (cdflag) {
                        d = b - diff;
                        fd = loglik(_y, d, sumLog);
                    }
                    if (fc > fd) {
                        a = d;
                        fa = fd;
                        d = c;
                        fd = fc;
                        cdflag = false;
                    } else {
                        b = c;
                        fb = fc;
                        c = d;
                        fc = fd;
                        cdflag = true;
                    }
                    if (fa > fb) {
                        power = a;
                    } else {
                        power = b;
                    }
                } else {
                    break;
                }
            }
            return power;
        }

        /// <summary>
        /// Charles D. Coleman, A fast, high-precision implementation of the univariate
        /// one-parameter Box_C0x transformation, NESUG 17 Analysis, p 1-4 using the
        /// golden section search .
        /// </summary>
        /// <param name="y"></param>
        /// <param name="lambda"></param>
        /// <param name="sumLog"></param>
        /// <returns></returns>
        private static double loglik(IEnumerable<double> y, double lambda, double sumLog) {
            var _y = y.ToList();
            var ty = new List<double>();
            var ssq = 0D;
            var n = _y.Count;

            if (Math.Abs(lambda) < 0.0001) {
                for (int i = 0; i < n; i++) {
                    ty.Add(Math.Log(_y[i]));
                }
            } else {
                for (int i = 0; i < n; i++) {
                    ty.Add((Math.Pow(_y[i], lambda) - 1) / lambda);
                }
            }
            var tyMean = ty.Average();
            for (int i = 0; i < n; i++) {
                ssq += Math.Pow(ty[i] - tyMean, 2) / n;
            }

            return -n * Math.Log(ssq) / 2 + (lambda - 1) * sumLog;
        }
    }
}
