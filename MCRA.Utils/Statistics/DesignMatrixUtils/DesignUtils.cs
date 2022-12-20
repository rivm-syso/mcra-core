using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils {
    public sealed class DesignUtils {

        /// <summary>
        /// The design matrix for LME4 differs from the matrix needed for LM.
        /// Therefore, a constant need to be added in case of LM.
        /// </summary>
        /// <param name="temp">The matrix to which the</param>
        /// <param name="n">Hint for the number of rows: used in case temp is empty
        /// (i.e., in this case the number of rows cannot be deduced from temp).</param>
        public static double[,] AddConstantColumn(double[,] temp, int n) {
            double[,] X = null;
            if (temp != null) {
                n = temp.GetLength(0);
                X = new double[n, temp.GetLength(1) + 1];
            } else {
                X = new double[n, 1];
            }
            for (int i = 0; i < n; i++) {
                X[i, 0] = 1;
                if (temp != null) {
                    for (int c = 1; c < X.GetLength(1); c++) {
                        X[i, c] = temp[i, c - 1];
                    }
                }
            }
            return X;
        }

        /// <summary>
        /// Generates a dummy matrix for the list of factor levels.
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public List<double[]> MakeDummy(IEnumerable<string> factor) {
            var design = new List<double[]>();
            foreach (var label in factor.Distinct().OrderBy(c =>c).Skip(1)) {
                var dummy = new List<double>();
                foreach (var item in factor) {
                    if (item.Equals(label)) {
                        dummy.Add(1);
                    } else {
                        dummy.Add(0);
                    }
                }
                design.Add(dummy.ToArray());
            }
            return design;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="covariable"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public List<double[]> MakeInteraction(
            IEnumerable<string> factor,
            List<double[]> covariable,
            int degree
        ) {
            var dum = MakeDummy(factor);
            var design = new List<double[]>();
            foreach (var item1 in dum) {
                for (int m = 0; m < degree; m++) {
                    var d = new List<double>();
                    for (int i = 0; i < covariable[m].Length; i++) {
                        d.Add(item1[i] * covariable[m][i]);
                    }
                    design.Add(d.ToArray());
                }
            }
            return design;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dummies"></param>
        /// <returns></returns>
        public double[,] ConvertToDesignMatrix(List<double[]> dummies) {
            var nCol = dummies.Count;
            if (nCol != 0) {
                var nRow = dummies.First().Length;
                var result = new double[nRow, nCol];
                for (int i = 0; i < nCol; i++) {
                    for (int j = 0; j < nRow; j++) {
                        result[j, i] = dummies[i][j];
                    }
                }
                return result;
            }
            return null;
        }
    }
}
