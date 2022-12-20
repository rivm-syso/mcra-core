using System.Collections.Generic;
using System.Linq;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation {
    public sealed class PercentilesUncertaintyFactorialResult {

        public double[,] DesignMatrix { get; set; }
        public List<double> Response { get; set; }
        public List<string> UncertaintySources { get; set; }

        public List<double> RegressionCoefficients { get; set; } = new List<double>();

        public double MeanDeviance { get; set; }
        public double ExplainedVariance { get; set; }

        public List<double> Contributions { get; private set; } = new List<double>();

        /// <summary>
        /// B = inv(X'X) * X'Y
        /// </summary>
        public void Calculate() {
            var n = Response.Count;
            var p = DesignMatrix.GetLength(1);
            var X = new GeneralMatrix(DesignMatrix);
            var Y = new GeneralMatrix(Response.ToArray(), n);
            var Xacc = X.Transpose();
            var B = Xacc.MultiplyOld(X).Inverse().MultiplyOld(Xacc).MultiplyOld(Y);
            RegressionCoefficients = B.ColumnPackedCopy.ToList();
            var XB = X.MultiplyOld(B);
            var deviance = Y.Subtract(XB).Transpose().MultiplyOld(Y.Subtract(XB));
            MeanDeviance = deviance.ColumnPackedCopy.Single() / (n - p);
            if (double.IsNaN(MeanDeviance)) {
                MeanDeviance = 0;
            }
            var variance = Response.Variance();
            if (double.IsNaN(variance)) {
                ExplainedVariance = double.NaN;
            } else {
                ExplainedVariance = (1 - MeanDeviance / variance);
            }
            var regressionCoefficients = RegressionCoefficients.Select(c => c > 0 ? c : 0).ToList();
            var sumContributions = regressionCoefficients.Sum();
            if (sumContributions > 0) {
                Contributions = regressionCoefficients.Select(c => c / sumContributions).ToList();
            }
        }
    }
}
