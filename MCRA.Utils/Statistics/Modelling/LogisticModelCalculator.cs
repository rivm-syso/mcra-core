using MCRA.Utils.NumericalRecipes;

namespace MCRA.Utils.Statistics.Modelling {

    /// <summary>
    /// Fits the Logistic-Normal model to binomial data by means of maximum Likelihood
    /// employing Gauss-Hermite integration. The dispersion parameter can be set to a
    /// fixed value by means of DispersionFix.
    /// </summary>
    /// <remarks>
    /// The current implementation can handle different covariates patterns within an
    /// individual, e.g. DayOfTheWeek can be used a covariable. However, is it not clear
    /// what the definition of ModelAssistedFrequency is in such a case. Therefor the
    /// implementation throws an exception when different covariate patterns are found.
    /// This restriction can be removed when an appropriate implementation of
    /// ModelAssistedFrequency is defined.
    /// </remarks>
    public class LogisticModelCalculator {

        private const double Invsqrtpi = 0.56418958354775628;

        /// <summary>
        /// Maximum number of function evaluations in Simplex Routine; default 5000.
        /// </summary>
        private readonly int _maxEvaluations = 1000;

        /// <summary>
        /// Convergence criterion in IRLS algorithm (default 1.0e-6). Operates on relative difference in log-likelihood between cycles.
        /// </summary>
        private readonly double _toleranceRelative = 1.0e-6;

        /// <summary>
        /// Number of quadrature points in one-dimension for Gauss-Hermite quadrature
        /// </summary>
        private readonly int _gaussHermitePoints = 32;

        /// <summary>
        /// One dimensional Gauss Hermite integration points and weights.
        /// </summary>
        private readonly double[,] _ghXW;

        /// <summary>
        /// Initializes the Class with default number of Gauss-Hermite integration points = 32.
        /// </summary>
        public LogisticModelCalculator() {
            _ghXW = UtilityFunctions.GaussHermite(_gaussHermitePoints);
        }

        /// <summary>
        /// Fit the Logistic Normal (LNN0) model with Weights.
        /// </summary>
        /// <param name="responses">Binomial response.</param>
        /// <param name="nBinomial">Binomial totals.</param>
        /// <param name="designMatrix">Design matrix for regression.</param>
        /// <param name="weights">Statistical weights (must be the same within individuals.</param>
        /// <param name="computeSes">Boolean to specify whether also to compute SEs.</param>
        /// <param name="fixedDispersion">Specify when running with fixed dispersion.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public LogisticModelCalculationResult Compute(
            List<int> responses,
            List<int> nBinomial,
            double[,] designMatrix,
            List<double> weights,
            bool computeSes = false,
            double? fixedDispersion = null
        ) {
            try {
                if ((responses == null) || (nBinomial == null) || (designMatrix == null)) {
                    throw new Exception("Response, Nbinomial, DesignMatrix and Individual must be set in LNN0fit.");
                }
                var nResponses = designMatrix.GetLength(0);
                var nPredictors = designMatrix.GetLength(1);
                if ((nResponses != nBinomial.Count) || (nResponses != designMatrix.GetLength(0)) || (nResponses != weights.Count)) {
                    throw new Exception("Response, Nbinomial DesignMatrix, Individual and Weights must have the same dimension in LNN0fit.");
                }

                // Archive for of parameter estimates and corresponding log-likelihoods.
                // Used for calculation of the hessian in the standard errors calculation
                // method.
                var evaluationsArchive = new List<(double[] Params, double LogLik)>();

                // Fit Logistic Regression for initial estimates
                var initialEstimates = LogisticRegressionCalculator
                    .Compute(
                        responses,
                        nBinomial,
                        designMatrix,
                        weights
                    );

                // Create simplex optimizer instance
                var Simplex = new OptimizeSimplex() {
                    Tolerance = _toleranceRelative,
                    MaxEvaluations = _maxEvaluations,
                };

                LogisticModelCalculationResult result;

                if (fixedDispersion.HasValue) {
                    // Optimisation on array of predictors

                    // Optimisation function
                    Func<double[], double> func = (x) => {
                        var estimates = x;
                        var logLikResult = calculateLogLikelihood(
                            responses,
                            nBinomial,
                            designMatrix,
                            weights,
                            estimates,
                            Math.Log(fixedDispersion.Value)
                        );
                        // Save current Estimates and corresponding LogLik (clone to assure copy)
                        evaluationsArchive.Add((x.Clone() as double[], logLikResult.loglik));
                        return logLikResult.loglik;
                    };

                    // Optimize
                    var xInit = initialEstimates.ToArray();
                    (var xOpt, var logLik) = Simplex.Minimize(xInit, func, 1.0);

                    // Extract estimates and dispersion from result
                    var estimates = xOpt.Take(nPredictors).ToList();
                    (_, var linearPredictor) = calculateLogLikelihood(
                        responses,
                        nBinomial,
                        designMatrix,
                        weights,
                        [.. estimates],
                        Math.Log(fixedDispersion.Value)
                    );

                    var degreesOfFreedom = weights.Sum() - estimates.Count;
                    result = new LogisticModelCalculationResult(
                        estimates,
                        fixedDispersion.Value,
                        logLik,
                        linearPredictor,
                        degreesOfFreedom,
                        false
                    );
                } else {
                    // Optimisation including dispersion parameter
                    // Find initial estimate for dispersion
                    var initialDispersion = computeInitialLogDispersion(
                        responses,
                        nBinomial,
                        designMatrix,
                        weights,
                        initialEstimates
                    );

                    // Optimisation on array of predictors + dispersion
                    var xInit = new double[nPredictors + 1];
                    initialEstimates.CopyTo(0, xInit, 0, nPredictors);
                    xInit[nPredictors] = initialDispersion;

                    // Optimisation function
                    Func<double[], double> func = (x) => {
                        // Estimates are first nPredictors elements
                        var estimates = x.Take(nPredictors).ToArray();

                        // Dispersion is last element on log-scale
                        var dispersion = x.Last();
                        var logLikResult = calculateLogLikelihood(
                            responses,
                            nBinomial,
                            designMatrix,
                            weights,
                            estimates,
                            dispersion
                        );

                        // Save current estimates+dispersion and corresponding LogLik to archive (clone to assure copy)
                        evaluationsArchive.Add((x.Clone() as double[], logLikResult.loglik));

                        return logLikResult.loglik;
                    };

                    // Optimize
                    (var xOpt, var logLik) = Simplex.Minimize(xInit, func, 1.0);

                    // Extract estimates and dispersion from result
                    var estimates = xOpt.Take(nPredictors).ToList();
                    var dispersion = Math.Exp(xOpt[nPredictors]);
                    (_, var linearPredictor) = calculateLogLikelihood(
                        responses,
                        nBinomial,
                        designMatrix,
                        weights,
                        [.. estimates],
                        xOpt[nPredictors]
                    );

                    var degreesOfFreedom = weights.Sum() - (estimates.Count + 1);
                    result = new LogisticModelCalculationResult(
                        estimates,
                        dispersion,
                        logLik,
                        linearPredictor,
                        degreesOfFreedom
                    );
                }

                if (computeSes) {
                    (var ses, var dispersionSe) = computeStandardErrors(
                        responses,
                        nBinomial,
                        designMatrix,
                        weights,
                        result.Estimates,
                        result.Dispersion,
                        evaluationsArchive,
                        !fixedDispersion.HasValue
                    );
                    result.StandardErrors = [.. ses];
                    result.DispersionStandardError = dispersionSe;
                }
                return result;
            } catch (Exception ee) {
                throw new Exception($"Logistic-Normal model (LNN0) aborted with the following exception: {ee.Message}");
            }
        }

        /// <summary>
        /// Computes the fitted values.
        /// </summary>
        public List<double> ComputeFittedValues(
            double[] linearPredictor,
            double dispersion,
            List<int> nBinomial,
            int nResponse
        ) {
            try {
                var factor = Math.Sqrt(2 * dispersion);
                var ghx = new double[_gaussHermitePoints];
                for (int k = 0; k < _gaussHermitePoints; k++) {
                    ghx[k] = factor * _ghXW[k, 0];
                }
                var fittedValues = new List<double>();
                for (int jj = 0; jj < nResponse; jj++) {
                    var lp = linearPredictor[jj];
                    var fit = 0D;
                    for (int ig = 0; ig < _gaussHermitePoints; ig++) {
                        fit += _ghXW[ig, 1] * UtilityFunctions.ILogit(lp + ghx[ig]);
                    }
                    fittedValues.Add(nBinomial[jj] * fit * Invsqrtpi);
                }
                return fittedValues;
            } catch (Exception ee) {
                throw new Exception($"Calculation of fitted Values for the Logistic-Normal model (LNN) " +
                    $"aborted with the following exception: {ee.Message}");
            }
        }

        /// <summary>
        /// Computes frequency estimates adjusted for the model
        /// (Model Assisted Frequency).
        /// </summary>
        public List<double> ComputeModelAssistedFrequency(
            double[] linearPredictor,
            double dispersion,
            List<int> responses,
            List<int> nBinomial
        ) {
            var nResponse = responses.Count;
            try {
                var factor = Math.Sqrt(2.0 * dispersion);
                var ghx = new double[_gaussHermitePoints];
                for (int k = 0; k < _gaussHermitePoints; k++) {
                    ghx[k] = factor * _ghXW[k, 0];
                }
                var modelAssistedFrequency = new List<double>();
                for (int i = 0; i < nResponse; i++) {
                    var lp = linearPredictor[i];
                    // It is assumed that the covariate pattern within individuals is the same.
                    // Otherwise it is not clear how to calculate a Model Assisted Frequency.
                    // Loop to get the binomial response for the individual.
                    var yyii = responses[i];
                    var nnii = nBinomial[i];
                    // Integrate over random effect
                    var denom = 0d;
                    var nom = 0d;
                    for (int ig = 0; ig < _gaussHermitePoints; ig++) {
                        var pp = UtilityFunctions.ILogit(lp + ghx[ig]);
                        var pr = Math.Pow(pp, yyii) * Math.Pow(1.0 - pp, nnii - yyii);
                        denom += _ghXW[ig, 1] * pr;
                        nom += pp * _ghXW[ig, 1] * pr;
                    }
                    modelAssistedFrequency.Add(nom / denom);
                }
                return modelAssistedFrequency;
            } catch (Exception ee) {
                throw new Exception($"Calculation of Model Assisted Frequencies for the Logistic-Normal model " +
                    $"(LNN)) aborted with the following exception: {ee.Message}");
            }
        }

        private LoglikelihoodResults calculateLogLikelihood(
            List<int> response,
            List<int> nBinomial,
            double[,] designMatrix,
            List<double> weights,
            double[] estimates,
            double logDispersion
        ) {
            var nresponse = designMatrix.GetLength(0);
            var npredictors = designMatrix.GetLength(1);

            var ghx = new double[_gaussHermitePoints];
            var factor = Math.Sqrt(2.0 * UtilityFunctions.ExpBound(logDispersion));
            for (int k = 0; k < _gaussHermitePoints; k++) {
                ghx[k] = factor * _ghXW[k, 0];
            }
            var logLik = 0D;

            var curRecord = 0;
            var countZeroLik = 0;
            var maxLogLik = double.MinValue;
            var linearPredictor = new double[nresponse];
            for (int i = 0; i < nresponse; i++) {
                var lp = 0d;
                for (int kk = 0; kk < npredictors; kk++) {
                    lp += estimates[kk] * designMatrix[curRecord, kk];
                }
                linearPredictor[curRecord] = lp;
                // Gauss Hermite integration for current individual
                var curLogLik = 0D;
                for (int ig = 0; ig < _gaussHermitePoints; ig++) {
                    var prob = 1D;
                    var pp = UtilityFunctions.ILogit(linearPredictor[curRecord] + ghx[ig]);
                    prob *= Math.Pow(pp, response[curRecord]) * Math.Pow(1.0 - pp, nBinomial[curRecord] - response[curRecord]);
                    curLogLik += _ghXW[ig, 1] * prob;
                }
                // Finalize log-likelihood
                if (curLogLik > 0) {
                    curLogLik = -weights[curRecord] * Math.Log(curLogLik * Invsqrtpi);
                    logLik += curLogLik;
                    maxLogLik = Math.Max(maxLogLik, curLogLik);
                } else {
                    countZeroLik += 1;
                }
                curRecord++;
            }
            if (countZeroLik > 0) {
                logLik += countZeroLik * maxLogLik;
            }

            return (logLik, linearPredictor);
        }

        private (List<double> estimates, double dispersion) computeStandardErrors(
            List<int> response,
            List<int> nBinomial,
            double[,] designMatrix,
            List<double> weights,
            List<double> estimates,
            double dispersion,
            List<(double[] Params, double LogLik)> archive,
            bool isEstimatedDispersion = true
        ) {
            try {
                var npredictors = designMatrix.GetLength(1);
                var parMin = new List<double>();
                if (isEstimatedDispersion) {
                    parMin.AddRange(estimates);
                    parMin.Add(Math.Log(dispersion));
                } else {
                    parMin.AddRange(estimates);
                }

                // Estimate the stepsize for the calculation of the second-order derivative
                // based on the archived list of function evaluations provided as argument.
                var stepderiv = StepSizeFunctions
                    .StepSize2(
                        archive.Select(r => r.LogLik).ToList(),
                        archive.Select(r => r.Params).ToList(),
                        archive.Last().LogLik,
                        archive.Last().Params,
                        2D,
                        0.1
                    );

                Func<double[], double> func = (x) => {
                    var logLikResult = calculateLogLikelihood(
                        response,
                        nBinomial,
                        designMatrix,
                        weights,
                        [.. x],
                        isEstimatedDispersion ? x.Last() : Math.Log(dispersion)
                    );
                    return logLikResult.loglik;
                };

                var derivativeMultiD = new DerivativeMultiD();
                var variance = derivativeMultiD
                    .Hessian(
                        (x, _) => func(x),
                        [.. parMin],
                        stepderiv,
                        out var error
                    );
                var varMat = new GeneralMatrix(variance);
                try {
                    varMat = varMat.Inverse();
                } catch (Exception) {
                    throw new Exception("NCI Variance-covariance matrix is probably singular");
                }

                var vcovarianceFull = new GeneralMatrix(npredictors + 1, npredictors + 1);

                if (isEstimatedDispersion) {
                    // Variance for dispersion parameter is on the Log-scale
                    var deriv = Enumerable.Repeat(1D, npredictors).ToList();
                    deriv.Add(dispersion);
                    var diagonal = GeneralMatrix.CreateDiagonal([.. deriv]);
                    vcovarianceFull = diagonal * varMat.NanReplace(0D) * diagonal;
                } else {
                    var elem = new int[npredictors];
                    for (int i = 0; i < npredictors + 1; i++) {
                        if (i < npredictors) {
                            elem[i] = i;
                        }
                        vcovarianceFull.SetElement(npredictors, i, double.NaN);
                        vcovarianceFull.SetElement(i, npredictors, double.NaN);
                    }
                    vcovarianceFull.SetMatrix(elem, elem, varMat);
                }

                // Collect SEs from variance-covariance matrix
                var ses = new List<double>();
                for (int i = 0; i < npredictors; i++) {
                    ses.Add(Math.Sqrt(vcovarianceFull.GetElement(i, i)));
                }
                var dispersionSe = Math.Sqrt(vcovarianceFull.GetElement(npredictors, npredictors));

                return (ses, dispersionSe);
            } catch (Exception ee) {
                throw new Exception($"Calculation of the Variance-Covariance matrix for the Logistic-Normal model (LNN0) " +
                    $"aborted with the following exception {ee.Message}");
            }
        }

        private double computeInitialLogDispersion(
            List<int> response,
            List<int> nBinomial,
            double[,] designMatrix,
            List<double> weights,
            List<double> initialEstimates
        ) {
            var mingrid = -4D;
            var maxgrid = 3D;
            var ngrid = 28;
            var incgrid = (maxgrid - mingrid) / ngrid;
            var disp = mingrid;
            var bestLogLik = double.MaxValue;
            var bestDisp = 0D;
            for (int i = 0; i < ngrid; i++) {
                var loglikelihoodResults = calculateLogLikelihood(
                    response,
                    nBinomial,
                    designMatrix,
                    weights,
                    [.. initialEstimates],
                    disp
                );
                if (loglikelihoodResults.loglik < bestLogLik) {
                    bestLogLik = loglikelihoodResults.loglik;
                    bestDisp = disp;
                }
                disp += incgrid;
            }
            return bestDisp;
        }

        internal record struct LoglikelihoodResults(
            double loglik,
            double[] linearPredictor
        ) {
            public static implicit operator (double loglik, double[] linearPredictor)(LoglikelihoodResults value) {
                return (value.loglik, value.linearPredictor);
            }

            public static implicit operator LoglikelihoodResults((double loglik, double[] linearPredictor) value) {
                return new LoglikelihoodResults(value.loglik, value.linearPredictor);
            }
        }
    }
}
