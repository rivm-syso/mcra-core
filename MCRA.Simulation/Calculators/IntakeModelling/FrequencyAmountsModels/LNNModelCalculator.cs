using log4net;
using MCRA.Simulation.Calculators.IntakeModelling.FrequencyAmountsModels;
using MCRA.Utils;
using MCRA.Utils.NumericalRecipes;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class LNNModelCalculator {

        private static readonly ILog log = LogManager.GetLogger(typeof(LNNModelCalculator));

        /// <summary>
        /// Number of quadrature points in one-dimension for Gauss-Hermite quadrature
        /// </summary>
        private const int GaussHermitePoints = 10;

        /// <summary>
        /// Criterion for pruning the quadrature points.
        /// </summary>
        private const double GaussHermitePrune = -1D;

        /// <summary>
        /// Maximum number of function evaluations in Simplex Routine; default 5000;
        /// </summary>
        private const int MaxEvaluations = 300;

        /// <summary>
        /// Convergence criterion in minimization routines; default 1e-6.
        /// </summary>
        private const double Tolerance = 1e-6;

        /// <summary>
        /// Two dimensional Gauss Hermite integration points and weights
        /// </summary>
        private readonly double[,] _ghXXW = SpecialFunctions.GaussHermiteTwoD(GaussHermitePoints, GaussHermitePrune);

        /// <summary>
        /// Number of units for which the likelihood contribution is zero; this is indicative for an error during the optimization routine
        /// </summary>
        private int _countZeroLik;

        /// <summary>
        /// Linear predictor for Frequency (transformed scale).
        /// </summary>
        private double[] _freqLp;

        /// <summary>
        /// Linear predictor for Amounts (transformed scale);
        /// </summary>
        private double[] _amountLp;

        /// <summary>
        /// Fits the LNN model to the provided data using the specified initial parameters.
        /// </summary>
        /// <param name="amounts"></param>
        /// <param name="weights">Subject sampling weights.</param>
        /// <param name="individualIntakeFrequencies"></param>
        /// <param name="freqDesign">Frequencies design matrix.</param>
        /// <param name="amountDesign">Amounts design matrix.</param>
        /// <param name="intakeTransformer"></param>
        /// <param name="initialParams"></param>
        /// <returns></returns>
        public LNNModelResult ComputeFit(
            List<double> amounts,
            List<double> weights,
            List<int> individualIntakeFrequencies,
            double[,] freqDesign,
            double[,] amountDesign,
            IntakeTransformer intakeTransformer,
            LNNParameters initialParams,
            bool computeSEs = true
        ) {
            log.Info("LNN model with correlation is fitted.");
            log.Info($"Maximum number of function evaluations: {MaxEvaluations} ; Tolerance: {Tolerance}");

            _freqLp = new double[amounts.Count];
            _amountLp = new double[amounts.Count];

            var numAmountParams = initialParams.AmountEstimates.Count;
            var numFrequencyParams = initialParams.FreqEstimates.Count;
            initialParams.ParametersT.Dispersion = UtilityFunctions.LogBound(initialParams.Parameters.Dispersion);
            initialParams.ParametersT.VarianceBetween = UtilityFunctions.LogBound(initialParams.Parameters.VarianceBetween);
            initialParams.ParametersT.VarianceWithin = UtilityFunctions.LogBound(initialParams.Parameters.VarianceWithin);
            initialParams.ParametersT.Correlation = UtilityFunctions.LogBound((1.0 + initialParams.Parameters.Correlation)
                / (1.0 - initialParams.Parameters.Correlation));
            var xInit = inverseClone(initialParams);

            var amountsTransformed = amounts
                .Select(c => c > 0 ? intakeTransformer.Transform(c) : double.NaN)
                .ToList();

            // Archive for of parameter estimates and corresponding log-likelihoods.
            // Used for calculation of the hessian in the standard errors calculation method.
            var evaluationsArchive = new List<(double[] Params, double LogLik)>();

            double computeLogLik(double[] x) {
                var estimates = clone(x, freqDesign.GetLength(1), amountDesign.GetLength(1));
                var logLik = calculateLogLikContribution(
                    amountsTransformed,
                    weights,
                    individualIntakeFrequencies,
                    freqDesign,
                    amountDesign,
                    estimates
                );

                // Save current estimates and corresponding LogLik to archive (clone to assure copy)
                evaluationsArchive.Add((x.Clone() as double[], logLik));
                return logLik;
            };
            var simplex = new OptimizeSimplex() {
                MaxEvaluations = MaxEvaluations,
                Tolerance = Tolerance
            };
            (var estimates, var logLik) = simplex.Minimize([.. xInit], computeLogLik, 1);
            var modelFit = clone(estimates, numFrequencyParams, numAmountParams);

            var errorMessage = ErrorMessages.Convergence;
            if (simplex.Convergence == false) {
                errorMessage = ErrorMessages.NoConvergence;
                log.Info($"Warning. The Simplex algorithm to fit the LNN model with correlation did not did not converge " +
                    $"within {MaxEvaluations} function evaluations using a tolerance of {Tolerance}.");
            } else if (_countZeroLik > 0) {
                errorMessage = ErrorMessages.Error;
                log.Info($"Warning. Results for the LNN model with correlation are probably wrong because {_countZeroLik} " +
                    $"observations have a Likelihood contribution of zero (and thus a Log-Likelihood contribution of minus infinity.");
            }

            var result = new LNNModelResult() {
                Parameters = modelFit,
                LogLik = logLik,
                ErrorMessages = errorMessage,
            };

            if (computeSEs) {
                var dim = 5 + numFrequencyParams + numAmountParams;
                var indices = Enumerable.Range(0, estimates.Length).ToArray();
                log.Info("Variance-Covariance matrix is being calculated for LNN model with Correlation.");

                var parMin = evaluationsArchive.Last().Params;
                var stepderiv = StepSizeFunctions.StepSize2(
                    evaluationsArchive.Select(r => r.LogLik).ToList(),
                    evaluationsArchive.Select(r => r.Params).ToList(),
                    evaluationsArchive.Last().LogLik,
                    parMin,
                    2D,
                    0.1
                );

                var derivativeMultiD = new DerivativeMultiD();
                var variance = derivativeMultiD
                    .Hessian(
                        (x, _) => computeLogLik(x),
                        [.. parMin],
                        stepderiv,
                        out var error
                    );
                var varMat = new GeneralMatrix(variance);
                try {
                    varMat = varMat.Inverse();
                    var vcovarianceT = new GeneralMatrix(dim, dim, double.NaN);
                    vcovarianceT.SetMatrix(indices, indices, varMat);
                    var vcovariance = computeVcov(
                        vcovarianceT,
                        modelFit
                    );
                    result.StandardErrors = computeStdErrors(vcovariance, vcovarianceT, modelFit);
                } catch {
                    result.ErrorMessages = ErrorMessages.ConvergenceNoStandardErrors;
                }
            }
            return result;
        }

        /// <summary>
        /// Clone multiple argument
        /// </summary>
        private LNNParameters clone(
            double[] args,
            int numFrequencyParams,
            int numAmountParams
        ) {
            var estimates = new LNNParameters() {
                AmountEstimates = [],
                FreqEstimates = [],
                Parameters = new(),
            };
            var ix = 0;
            for (int i = 0; i < numFrequencyParams; i++) {
                estimates.FreqEstimates.Add(args[ix++]);
            }
            for (int i = 0; i < numAmountParams; i++) {
                estimates.AmountEstimates.Add(args[ix++]);
            }
            estimates.ParametersT.Dispersion = args[ix++];
            estimates.ParametersT.VarianceBetween = args[ix++];
            estimates.ParametersT.VarianceWithin = args[ix++];
            estimates.ParametersT.Correlation = args[ix++];
            estimates.InverseTransform();
            return estimates;
        }

        /// <summary>
        /// Fill structure
        /// </summary>
        /// <returns></returns>
        private static List<double> inverseClone(LNNParameters lnn) {
            var parameters = new List<double>();
            for (int i = 0; i < lnn.FreqEstimates.Count; i++) {
                parameters.Add(lnn.FreqEstimates[i]);
            }
            for (int i = 0; i < lnn.AmountEstimates.Count; i++) {
                parameters.Add(lnn.AmountEstimates[i]);
            }
            parameters.Add(lnn.ParametersT.Dispersion);
            parameters.Add(lnn.ParametersT.VarianceBetween);
            parameters.Add(lnn.ParametersT.VarianceWithin);
            parameters.Add(lnn.ParametersT.Correlation);
            return parameters;
        }

        /// <summary>
        /// Calculates the log-likelihood contribution for the current dataset using the specified latent normal-normal
        /// (LNN) model parameters. 
        /// </summary>
        /// <remarks>This method performs double Gauss-Hermite integration over all individuals in the
        /// dataset to compute the likelihood contribution. If any individual yields a zero likelihood, the method
        /// adjusts the total log-likelihood to account for these cases. The method assumes that all required data
        /// structures and model design matrices are properly initialized before invocation.</remarks>
        /// <param name="lnn">The set of LNN model parameters to use for the likelihood calculation. The parameters must be properly
        /// initialized and transformed as required by the model.</param>
        /// <returns>The total log-likelihood contribution as a double-precision floating-point value. Higher values indicate a
        /// better fit of the model to the observed data.</returns>
        private double calculateLogLikContribution(
            List<double> dailyIntakesTransformed,
            List<double> weights,
            List<int> individualIntakeFrequencies,
            double[,] freqDesign,
            double[,] amountDesign,
            LNNParameters lnn
        ) {
            // Back-transform parameters
            lnn.InverseTransform();
            var parameters = lnn.Parameters;

            // Create rotation parameters and scale Gauss Hermite Integration points
            var nPoints = _ghXXW.GetLength(0);
            var aa = (Math.Sqrt(1 + parameters.Correlation) + Math.Sqrt(1 - parameters.Correlation)) / Math.Sqrt(2);
            var bb = (Math.Sqrt(1 + parameters.Correlation) - Math.Sqrt(1 - parameters.Correlation)) / Math.Sqrt(2);
            var aaX = new double[nPoints];
            var bbX = new double[nPoints];
            for (int ii = 0; ii < nPoints; ii++) {
                aaX[ii] = aa * _ghXXW[ii, 0] + bb * _ghXXW[ii, 1];
                bbX[ii] = aa * _ghXXW[ii, 1] + bb * _ghXXW[ii, 0];
            }

            // Loop over Individuals to determine the likelihood contribution for each individual
            var sqrtFreqVarIndividuals = Math.Sqrt(parameters.Dispersion);
            var sqrtAmountVarBetween = Math.Sqrt(parameters.VarianceBetween);
            var sqrtAmountVarWithin = Math.Sqrt(parameters.VarianceWithin);
            var startIndiv = 0;
            _countZeroLik = 0;
            var maxLogLik = double.MinValue;
            var logLikelihood = 0D;

            for (int i = 0; i < individualIntakeFrequencies.Count; i++) {
                var nDays = individualIntakeFrequencies[i];
                // Linear predictors for Frequency and Amount
                for (int j = 0; j < nDays; j++) {
                    var curRecord = startIndiv + j;
                    _freqLp[curRecord] = getContributionLP(lnn.FreqEstimates, freqDesign, curRecord);
                    _amountLp[curRecord] = getContributionLP(lnn.AmountEstimates, amountDesign, curRecord);
                }
                // Double Gauss Hermite Integration
                var curLogLik = 0D;
                for (int ig = 0; ig < nPoints; ig++) {
                    var prob = 1D;
                    for (int j = 0; j < nDays; j++) {
                        var curRecord = startIndiv + j;
                        var pp = UtilityFunctions.ILogit(sqrtFreqVarIndividuals * aaX[ig] + _freqLp[curRecord]);
                        if (double.IsNaN(dailyIntakesTransformed[curRecord])) {
                            prob *= (1 - pp);
                        } else {
                            var dAmount = dailyIntakesTransformed[curRecord];
                            var contrib = UtilityFunctions.PRNormal(sqrtAmountVarBetween * bbX[ig] + _amountLp[curRecord] - dAmount, 0D, lnn.Parameters.VarianceWithin, sqrtAmountVarWithin);
                            prob *= pp * contrib;
                        }
                    }
                    curLogLik += _ghXXW[ig, 2] * prob;
                }

                if (curLogLik > 0) {
                    curLogLik = -Math.Log(curLogLik / Math.PI) * weights[startIndiv];
                    logLikelihood += curLogLik;
                    maxLogLik = Math.Max(maxLogLik, curLogLik);
                } else {
                    _countZeroLik += 1;
                }
                startIndiv += nDays;
            }
            if (_countZeroLik > 0) {
                logLikelihood += _countZeroLik * maxLogLik;
            }
            return logLikelihood;
        }

        /// <summary>
        /// Returns a prediction on the linear scale
        /// </summary>
        /// <param name="estimates"></param>
        /// <param name="design"></param>
        /// <param name="curRecord"></param>
        /// <returns></returns>
        private static double getContributionLP(List<double> estimates, double[,] design, int curRecord) {
            var result = 0D;
            for (int k = 0; k < design.GetLength(1); k++) {
                result += estimates[k] * design[curRecord, k];
            }
            return result;
        }

        /// <summary>
        /// Creates Vcovariance from VcovarianceT
        /// </summary>
        private static GeneralMatrix computeVcov(
            GeneralMatrix vcovT,
            LNNParameters lnn
        ) {
            var deriv = new List<double>();
            for (int i = 0; i < (lnn.FreqEstimates.Count + lnn.AmountEstimates.Count); i++) {
                deriv.Add(1D);
            }
            var parameters = lnn.Parameters;
            deriv.Add(parameters.Dispersion);
            deriv.Add(parameters.VarianceBetween);
            deriv.Add(parameters.VarianceWithin);
            var pp = (parameters.Correlation + 1) / 2;
            deriv.Add(2 * pp * (1 - pp));
            deriv.Add(parameters.Power);
            var diagonal = GeneralMatrix.CreateDiagonal([.. deriv]);
            var vcov = diagonal * vcovT.NanReplace(0D) * diagonal;
            vcov = vcov.NanInsert(vcovT);
            return vcov;
        }

        private static LNNParameters computeStdErrors(
            GeneralMatrix vcov,
            GeneralMatrix vcovT,
            LNNParameters lnn
        ) {
            var stdErrors = new LNNParameters {
                FreqEstimates = [],
                AmountEstimates = []
            };
            var ix = 0;
            for (int i = 0; i < lnn.FreqEstimates.Count; i++) {
                stdErrors.FreqEstimates.Add(Math.Sqrt(vcovT.GetElement(ix, ix++)));
            }
            for (int i = 0; i < lnn.AmountEstimates.Count; i++) {
                stdErrors.AmountEstimates.Add(Math.Sqrt(vcovT.GetElement(ix, ix++)));
            }
            var parameters = stdErrors.Parameters;
            var parametersT = stdErrors.ParametersT;
            //Keep order like this otherwise wrong elements are copied
            parametersT.Dispersion = Math.Sqrt(vcovT.GetElement(ix, ix));
            parameters.Dispersion = Math.Sqrt(vcov.GetElement(ix, ix++));

            parametersT.VarianceBetween = Math.Sqrt(vcovT.GetElement(ix, ix));
            parameters.VarianceBetween = Math.Sqrt(vcov.GetElement(ix, ix++));

            parametersT.VarianceWithin = Math.Sqrt(vcovT.GetElement(ix, ix));
            parameters.VarianceWithin = Math.Sqrt(vcov.GetElement(ix, ix++));

            parametersT.Correlation = Math.Sqrt(vcovT.GetElement(ix, ix));
            parameters.Correlation = Math.Sqrt(vcov.GetElement(ix, ix++));

            parametersT.Power = Math.Sqrt(vcovT.GetElement(ix, ix));
            parameters.Power = Math.Sqrt(vcov.GetElement(ix, ix++));
            stdErrors.Parameters = parameters;
            stdErrors.ParametersT = parametersT;
            return stdErrors;
        }
    }
}
