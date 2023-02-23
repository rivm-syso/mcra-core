namespace MCRA.Utils.NumericalRecipes {
    //[Obsolete("eruit", true)]
    public class MatrixNR {

        /// <summary>
        /// Cholesky decomposition of a symmetric matrix.
        /// </summary>
        /// <param name="matrix">Input matrix (must be positive definite)</param>
        /// <returns>Cholesky decomposition.</returns>
        /// <remarks>Numerical Recipes routine CHOLDC ported from C++ to C#.</remarks>
        public static double[,] Cholesky(double[,] matrix) {
            int nrows = matrix.GetLength(0);
            double[,] returnChol = new double[nrows, nrows];
            double[] diagonal = new double[nrows];
            double sum;
            int ii, jj, kk;
            // Copy input matrix to output matrix
            for (ii = 0; ii < nrows; ii++) {
                for (jj = 0; jj < nrows; jj++) {
                    returnChol[ii, jj] = matrix[ii, jj];
                }
            }
            // Ported from CHOLDC
            for (ii = 0; ii < nrows; ii++) {
                for (jj = ii; jj < nrows; jj++) {
                    for (sum = returnChol[ii, jj], kk = ii - 1; kk >= 0; kk--) {
                        sum -= returnChol[ii, kk] * returnChol[jj, kk];
                    }

                    if (ii == jj) {
                        if (sum <= 0.0) {
                            throw new Exception("The Cholesky decomposition failed.");
                        }

                        diagonal[ii] = Math.Sqrt(sum);
                    }
                    else {
                        returnChol[jj, ii] = sum / diagonal[ii];
                    }
                }
            }
            // Return full matrix
            for (ii = 0; ii < nrows; ii++) {
                returnChol[ii, ii] = diagonal[ii];
                for (jj = ii + 1; jj < nrows; jj++) {
                    returnChol[ii, jj] = returnChol[jj, ii];
                }
            }
            return returnChol;
        }
    }
}
