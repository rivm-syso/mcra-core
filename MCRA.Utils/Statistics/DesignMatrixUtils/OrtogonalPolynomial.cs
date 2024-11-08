namespace MCRA.Utils {

    /// <summary>
    ///
    /// </summary>
    public class OrthogonalPolynomial {

        /// <summary>
        /// Calculation of orthogonal polynomials and values for predictions based on orthogonal polynomial.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="maxDegree"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public Polynomial CalculateOrthPol(IEnumerable<double> x, int maxDegree, IEnumerable<double> w) {
            var weight = w.ToArray();
            var pol = new Polynomial();
            var n = x.Count();

            ////Scale x before calculation of polynomial to avoid numerical problems
            var mu = 0D;
            var ss = 0D;
            var i = 0;
            var pol1 = new double[n];

            foreach (var item in x) {
                mu += item * weight[i];
                i++;
            }

            mu = mu / weight.Sum();
            i = 0;
            foreach (var item in x) {
                ss += Math.Pow((item - mu), 2) * weight[i];
                i++;
            }

            var stdDev = Math.Sqrt(ss / (weight.Sum() - 1));
            if (stdDev == 0) {
                throw new Exception("polynomial is not allowed when all covariable levels are equal");
            }

            i = 0;
            foreach (var item in x) {
                pol1[i] = (item - mu) / stdDev;
                i++;
            }

            if (maxDegree >= n) {
                throw new Exception("number of degrees in polynomial should be smaller than the number of observations");
            }

            var p0 = new double[n];
            var p1 = new double[n];
            var sum0 = 1D;
            var sum1 = 0D;
            for (i = 0; i < n; i++) {
                p1[i] = 1;
                sum1 += weight[i];
            }
            var r1 = new List<double>();
            var r2 = new List<double>();
            var result = new List<double[]>();
            for (int m = 0; m < maxDegree; m++) {
                var p = CalcPolynomial(pol1, weight, ref p1, ref p0, ref sum0, ref sum1, ref r1, ref r2);
                result.Add(p);
            }
            pol.Result = result;
            pol.Coefficient1 = r1;
            pol.Coefficient2 = r2;
            pol.Mu = mu;
            pol.StdDev = stdDev;
            return pol;
        }

        /// <summary>
        /// Calculation of the components of a polynomial, starting with the first component onto the last (maxDegree).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="wt"></param>
        /// <param name="p1"></param>
        /// <param name="p0"></param>
        /// <param name="sum0"></param>
        /// <param name="sum1"></param>
        /// <param name="coefficient1"></param>
        /// <param name="coefficient2"></param>
        /// <returns></returns>
        double[] CalcPolynomial(IEnumerable<double> x, double[] wt, ref double[] p1, ref double[] p0, ref double sum0, ref double sum1, ref List<double> coefficient1, ref List<double> coefficient2) {
            var sum = 0D;
            var n = p1.Length;
            var p = new double[n];
            var component = new double[n];
            var ii = 0;

            foreach (var item in x) {
                p[ii] = item * p1[ii];
                sum += p[ii] * p1[ii] * wt[ii];
                ii++;
            }
            var regrCoef1 = sum / sum1;
            var regrCoef2 = sum1 / sum0;
            for (int i = 0; i < n; i++) {
                component[i] = p[i] - regrCoef1 * p1[i] - regrCoef2 * p0[i];
            }
            sum0 = sum1;
            sum1 = 0;
            for (int i = 0; i < n; i++) {
                sum1 += component[i] * component[i] * wt[i];
            }
            p0 = (double[])p1.Clone();
            p1 = (double[])component.Clone();
            coefficient1.Add(regrCoef1);
            coefficient2.Add(regrCoef2);
            return component;
        }
    }
}
