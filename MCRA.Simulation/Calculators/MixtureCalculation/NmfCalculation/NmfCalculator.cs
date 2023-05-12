using MCRA.Simulation.OutputGeneration;
using MCRA.Utils;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation {
    public sealed class NmfCalculator {
        private readonly INmfCalculatorSettings _settings;
        private readonly double _delta = 0.01;
        private readonly int _bigDelta = 1;

        private int[] index;
        private GeneralMatrix U;
        private GeneralMatrix V;
        private GeneralMatrix M;

        public NmfCalculator(INmfCalculatorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Sparse Nonnegative Matrix Underapproximation (SNMU).
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="random"></param>
        /// <param name="progressState"></param>
        public (List<ComponentRecord>, GeneralMatrix, GeneralMatrix, List<double>) Compute(
                GeneralMatrix exposure,
                IRandom random,
                ProgressState progressState
            ) {
            var lambda = new List<double>();
            for (int i = 0; i < _settings.NumberOfComponents; i++) {
                lambda.Add(_settings.Sparseness);
            }
            return Calculate(exposure, lambda, progressState, random);
        }

        /// <summary>
        /// Sparse Nonnegative Matrix Underapproximation (SNMU).
        /// </summary>
        /// <param name="exposureMatrix"></param>
        /// <param name="lambda"></param>
        /// <param name="progressState"></param>
        /// <param name="random"></param>
        public (List<ComponentRecord>, GeneralMatrix, GeneralMatrix, List<double>) Calculate(
            GeneralMatrix exposureMatrix,
            List<double> lambda,
            ProgressState progressState,
            IRandom random
        ) {
            progressState.Update("Sparse NMU model is calculated.");

            var rdim = exposureMatrix.RowDimension;
            var cdim = exposureMatrix.ColumnDimension;
            var numberOfComponents = Math.Min(_settings.NumberOfComponents, rdim);

            U = new GeneralMatrix(rdim, numberOfComponents);
            V = new GeneralMatrix(cdim, numberOfComponents);
            index = Enumerable.Range(0, rdim).ToArray();

            M = exposureMatrix.Copy();
            var totalVariation = exposureMatrix.ColumnPackedCopy.Select(c => c * c).Sum();

            //NMU Recursion
            ComponentRecord record = null;
            var componentRecords = new List<ComponentRecord>();
            var rmse = new List<double>();
            for (int k = 0; k < numberOfComponents; k++) {
                record = getSNMU(k, lambda[k], random);
                var mTild = U.Multiply(V.Transpose());
                rmse.Add(Math.Sqrt(exposureMatrix.Subtract(mTild).ColumnPackedCopy.Select(c => c * c).Sum() / (rdim * cdim)));
                componentRecords.Add(record);

            }
            var cumExplainedVariation = 0D;
            for (int i = 0; i < componentRecords.Count; i++) {
                var explainedVariation = 100 - componentRecords[i].Variance / totalVariation * 100 - cumExplainedVariation;
                cumExplainedVariation += explainedVariation;
                componentRecords[i].CumulativeExplainedVariance = cumExplainedVariation;
                componentRecords[i].Variance = explainedVariation;
            }
            return (componentRecords, U, V, rmse);
        }

        /// <summary>
        /// Sparse NMU
        /// </summary>
        /// <param name="k"></param>
        /// <param name="lambda0"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private ComponentRecord getSNMU(int k, double lambda0, IRandom random) {
            GeneralMatrix X = null;
            GeneralMatrix Y = null;
            var lam0 = lambda0;
            var sparseness0 = 0d;
            var convergence0 = 1e10d;
            var rdim = M.RowDimension;
            var cdim = M.ColumnDimension;
            var w = new List<double>();

            //initialization of (x, y, sigma)
            for (int i = 0; i < rdim; i++) {
                w.Add(random.NextDouble(0, 1));
            }
            X = new GeneralMatrix(w.ToArray(), rdim);
            var tM = M.Transpose();
            Y = tM.Multiply(X);
            Y = Y.ArrayRightDivideAssign(Y.NormF());

            X = M.Multiply(Y);
            var sigma = X.NormF();
            X = X.ArrayRightDivideAssign(sigma);

            //initialization of (x, y, sigma) with optimal rank-one NMF of V using power method
            //Equal to absolute value of first column of singular value decomposition M.SDV()
            for (int i = 0; i < 100; i++) {
                Y = tM.Multiply(X);
                Y = Y.ArrayRightDivideAssign(Y.NormF());
                X = M.Multiply(Y);
                sigma = X.NormF();
                X = X.ArrayRightDivideAssign(sigma);
            }

            U.SetMatrix(0, rdim - 1, k, k, X);
            V.SetMatrix(0, cdim - 1, k, k, Y.Multiply(sigma));

            //Initialization of Lagrangian variable lambda, performed by block to decrease memory requirement
            var Lambda = new GeneralMatrix(rdim, cdim);
            var YCopy = Y.ColumnPackedCopy;

            index.AsParallel()
               .ForAll(ix => {
                   for (int i = 0; i < cdim; i++) {
                       //r3
                       var difference = sigma * X.Array[ix][0] * YCopy[i] - M.Array[ix][i];
                       Lambda.Array[ix][i] = Math.Max(0, difference);
                   }
               });

            //Alternating optimization
            var iter = 0;
            var xS = X;
            var yS = Y;
            var sigmaS = sigma;
            var xP = X;
            var yP = Y;
            var maximumX = new double[rdim];

            var convergence1 = 0D;
            var sparseness1 = 0D;
            var lam = double.NaN;

            //lamNew = ii / 10.0;
            while (iter < _settings.NumberOfIterations && convergence0 > _settings.Epsilon) {
                //(x,y) Update
                if (iter == 0) {
                    maximumX = M.Multiply(Y).Subtract(Lambda.Multiply(Y)).ReplaceNegativeAssign().ColumnPackedCopy;
                    lam = lam0 * maximumX.Max();
                    maximumX = maximumX.Select(c => c - lam).Select(c => Math.Max(0, c)).ToArray();
                    X = new GeneralMatrix(maximumX, rdim);
                    X = X.ArrayRightDivideAssign(X.NormF());
                } else {
                    //r6
                    maximumX = M.Multiply(Y).Subtract(Lambda.Multiply(Y)).ReplaceNegativeAssign().ColumnPackedCopy;
                    var maximum = maximumX.Max();
                    if (maximum < lam) {
                        lam = Math.Max(0.01, 0.99 * maximum);
                    }
                    //r10
                    maximumX = maximumX.Select(c => c - lam).Select(c => Math.Max(0, c)).ToArray();
                    X = new GeneralMatrix(maximumX, rdim);
                    X = X.ArrayRightDivideAssign(X.NormF());
                }

                //decresase or increase mu
                var XCopy = X.ColumnPackedCopy;
                if (XCopy.Count(c => c > 0) < _delta * rdim) {
                    lam *= 0.95;
                } else if (XCopy.Count(c => c > 0) > _bigDelta * rdim) {
                    lam *= 1.05;
                }
                //r16
                var maximumY = tM.Multiply(X).Subtract(Lambda.Transpose().Multiply(X)).ReplaceNegativeAssign().ColumnPackedCopy;
                Y = new GeneralMatrix(maximumY, cdim);
                Y = Y.ArrayRightDivideAssign(Y.NormF());
                var tX = X.Transpose();
                //r17
                sigma = tX.Multiply(M).Subtract(tX.Multiply(Lambda)).Multiply(Y).ColumnPackedCopy.First();
                //Check if the solution is non-trivial
                if (sigma > 0) {
                    var mX = XCopy.Max();
                    U.SetMatrix(0, rdim - 1, k, k, X.ArrayRightDivide(mX));
                    V.SetMatrix(0, cdim - 1, k, k, Y.Multiply(mX * sigma));
                } else {
                    Lambda = Lambda.Multiply(0.95);
                    Y = new GeneralMatrix(V.Transpose().Array[k].ToArray(), 1);
                }
                //}
                //Convergence criterium
                sparseness1 = XCopy.Count(c => c == 0) / (1.0 * rdim);

                convergence1 = (xP.Subtract(X).NormF() + yP.Subtract(Y).NormF()) / 2;

                if ((convergence1 < convergence0) & !(1.75 * sparseness1 < sparseness0)) {
                    xS = X;
                    yS = Y;
                    sigmaS = sigma;
                } else if (iter == _settings.NumberOfIterations - 1 && (convergence1 > 10 * convergence0)) {
                    X = xS;
                    Y = yS;
                    sigmaS = sigma;
                    var mX = X.ColumnPackedCopy.Max();
                    U.SetMatrix(0, rdim - 1, k, k, X.ArrayRightDivide(mX));
                    V.SetMatrix(0, cdim - 1, k, k, Y.Multiply(sigmaS * mX));
                    iter = _settings.NumberOfIterations;
                }

                //Lambda update
                YCopy = Y.ColumnPackedCopy;
                index.AsParallel()
                    .ForAll(ix => {
                        for (int i = 0; i < cdim; i++) {
                            //changed on 30 aug 2022 (iter + 2) => (iter + 1)
                            var difference = Lambda.Array[ix][i] + (sigma * X.Array[ix][0] * YCopy[i] - M.Array[ix][i]) / (iter + 1);
                            Lambda.Array[ix][i] = Math.Max(0, difference);
                        }
                    });
                xP = X;
                yP = Y;
                convergence0 = convergence1;
                sparseness0 = sparseness1;

                iter++;
            }

            var u = new GeneralMatrix(U.Transpose().Array[k].ToArray(), rdim);
            var v = new GeneralMatrix(V.Transpose().Array[k].ToArray(), 1);
            // variation includes error: X = U * Transpose(V) + E
            M = M.Subtract(u.Multiply(v)).ReplaceNegativeAssign();

            return new ComponentRecord() {
                ComponentNumber = k + 1,
                Sparseness = sparseness1,
                Convergence = convergence1,
                Variance = M.ColumnPackedCopy.Select(c => c * c).Sum(),
                Iteration = iter,
                NumberOfSubstances = u.ColumnPackedCopy.Where(c => c > 0).Count(),
            };
        }
    }
}
