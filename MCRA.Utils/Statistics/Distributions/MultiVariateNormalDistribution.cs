namespace MCRA.Utils.Statistics {

    /// <summary>
    ///  Continuous Multivariate Normal distribution.
    ///  </summary>
    public sealed class MultiVariateNormalDistribution  {

        /// <summary>
        /// Multiple draws from multivariate Normal distribution.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="varCovar"></param>
        /// <param name="sampleSize"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static double[,] Draw(List<double> mean, double[,] varCovar, int sampleSize, IRandom random) {
            var allResults = new double[sampleSize, mean.Count];
            for (int i = 0; i < sampleSize; i++) {
                var result = Draw(mean, varCovar, random);
                for (int ii = 0; ii < mean.Count; ii++) {
                    allResults[i, ii] = result[ii];
                }
            }
            return allResults;
        }

        /// <summary>
        /// One draw from multivariate Normal distribution.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="mean"></param>
        /// <param name="varCovar"></param>
        /// <returns></returns>
        public static double[] Draw(List<double> mean, double[,] varCovar, IRandom random) {
            var tmp = new double[mean.Count];
            var result = new double[mean.Count];
            // Generate and Back-Transform by Cholesky
            for (int ii = 0; ii < mean.Count; ii++) {
                tmp[ii] = NormalDistribution.Draw(random, 0, 1);
                var dtmp = mean[ii];
                for (int jj = 0; jj <= ii; jj++) {
                    dtmp += tmp[jj] * varCovar[ii, jj];
                }
                result[ii] = dtmp;
            }
            return result;
        }
    }
}
