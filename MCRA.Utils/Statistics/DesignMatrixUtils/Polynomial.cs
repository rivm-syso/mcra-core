namespace MCRA.Utils {

    public class Polynomial {

        public List<double[]> Result { get; set; }
        public List<double> Coefficient1 { get; set; }
        public List<double> Coefficient2 { get; set; }
        public double Mu { get; set; }
        public double StdDev { get; set; }

        /// <summary>
        /// Calculate polynomial components based on supplied values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="maxDegree"></param>
        /// <returns></returns>
        public List<double[]> CalculateComponentsPolynomial(IEnumerable<double> x, int maxDegree) {
            if (maxDegree > Coefficient1.Count()) {
                throw new Exception("Number of degrees in CalcPolynomial should be smaller than the number of coefficients<br>");
            }
            var n = x.Count();
            var p0 = new double[n];
            var p1 = new double[n];
            var pol1 = new double[n];
            var result = new List<double[]>();

            var ii = 0;
            foreach (var item in x) {
                p1[ii] = 1;
                pol1[ii] = (item - Mu) / StdDev;
                ii++;
            }

            for (int i = 0; i < maxDegree; i++) {
                var tmp = new double[n];
                for (int j = 0; j < n; j++) {
                    tmp[j] = pol1[j] * p1[j] - Coefficient1[i] * p1[j] - Coefficient2[i] * p0[j];
                    p0[j] = p1[j];
                    p1[j] = tmp[j];
                }
                result.Add(tmp);
            }
            return result;
        }
    }
}
