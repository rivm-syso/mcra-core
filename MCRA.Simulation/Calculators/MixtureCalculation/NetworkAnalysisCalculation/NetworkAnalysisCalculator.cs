using MCRA.Utils;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation {
    public sealed class NetworkAnalysisCalculator {

        private readonly double _epsPercentage = 0.01;
        private readonly bool _isLogTransform;

        public NetworkAnalysisCalculator(bool isLogTransform = false) {
            _isLogTransform = isLogTransform;
        }

        public double[,] Compute(
            GeneralMatrix rawExposureMatrix
        ) {
            var exposureMatrix = new GeneralMatrix(rawExposureMatrix.RowDimension, rawExposureMatrix.ColumnDimension);
            if (_isLogTransform) {
                for (int i = 0; i < rawExposureMatrix.RowDimension; i++) {
                    var logReplacement = Math.Log(rawExposureMatrix.Array[i].Where(c => c > 0).Min() * _epsPercentage);
                    for (int j = 0; j < rawExposureMatrix.ColumnDimension; j++) {
                        exposureMatrix.Array[i][j] = rawExposureMatrix.Array[i][j] > 0
                            ? Math.Log(rawExposureMatrix.Array[i][j]) : logReplacement;
                    }
                }
            } else {
                exposureMatrix = rawExposureMatrix;
            }
            var scaledMatrix = exposureMatrix.ScaleRows();
            var glassoSelect = new double[scaledMatrix.RowDimension, scaledMatrix.RowDimension];
            using (var R = new RDotNetEngine()) {
                R.LoadLibrary($"huge", null, true);
                R.LoadLibrary($"glasso", null, true);
                R.SetSymbol("data_scaled", scaledMatrix.TransposeArrayCopy2);
                R.EvaluateNoReturn("glasso.fit <- huge(data_scaled, method = \"glasso\", nlambda = 10, lambda.min.ratio = 0.1)");
                //Default number of subsamples; unclear from paper what value was used
                R.EvaluateNoReturn("glasso.select <- huge.select(glasso.fit, criterion = \"stars\", stars.thresh = 0.05, rep.num = 20)");
                glassoSelect = R.EvaluateMatrix("glasso.select$opt.icov");
            }
            return glassoSelect;
        }
    }
}
