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
    public class LogisticCalculator {

        /// <summary>
        /// Class internal to LogisticNormalModel
        /// </summary>
        internal class DataLogisticNormal {
            // Binomial Response
            public List<int> Response { get; set; }

            // Binomial Totals
            public List<int> Nbinomial { get; set; }

            // DesignMatrix which includes the constant term
            public double[,] DesignMatrix { get; set; }

            // Individual random effect
            public List<int> Individual { get; set; }

            // Statistical weights
            public List<double> Weights { get; set; }

            // Numer of individuals
            public int CountIndividual { get; set; }

            // Number of observations for each individual
            public int[] NobsPerIndividual { get; set; }

            // Current estimates of regression parameters
            public List<double> CurrentEstimates { get; set; }

            // Current estimate of dispersion parameter
            public double CurrentDispersion { get; set; }

            // Dispersion parameter, set to NaN to estimate the dispersion parameter.
            public double DispersionFix { get; set; }
        }

        private const double invsqrtpi = 0.56418958354775628;

        /// <summary>Maximum number of function evaluations in Simplex Routine; default 5000.</summary>
        public int MaxEvaluations { get; set; }
        /// <summary>Convergence criterion in IRLS algorithm (default 1.0e-6). Operates on absolute difference in log-likelihood between cycles.</summary>
        public double ToleranceAbsolute { get; set; }
        /// <summary>Convergence criterion in IRLS algorithm (default 1.0e-6). Operates on relative difference in log-likelihood between cycles.</summary>
        public double ToleranceRelative { get; set; }
        /// <summary>Dispersion parameter (default NaN); set to NaN to estimate the dispersion parameter.</summary>
        public double DispersionFix { get; set; }
        /// <summary>Maximum number of cycles for calculation of Standard Errors and Variance-covariance matrix (default 2).</summary>
        public int SeMaxCycle { get; set; }

        /// <summary>Estimates (excluding dispersion)</summary>
        public List<double> Estimates { get; private set; }

        private double _dispersion = double.NaN;
        /// <summary>Estimate of Dispersion parameter, i.e. the variance on the logit scale.</summary>
        public double Dispersion {
            get { return _dispersion; }
            set { _dispersion = value; }
        }

        //public double Dispersion { get; private set; }

        private bool seCalculated;
        private double[] se;
        /// <summary>Standard errors of estimates (excluding dispersion).</summary>
        public double[] Se {
            get {
                if (!seCalculated) {
                    CalculateStandardErrors();
                }
                return se;
            }
            private set { se = value; }
        }
        private double dispersionSe;
        /// <summary>Standard error of Dispersion parameter</summary>
        public double DispersionSe {
            get {
                if (!double.IsNaN(DispersionFix)) {
                    return double.NaN;
                }
                if (!seCalculated) {
                    CalculateStandardErrors();
                }
                return dispersionSe;
            }
            private set { dispersionSe = value; }
        }
        private GeneralMatrix vcovariance;
        /// <summary>Variance-covariance matrix of estimates (excluding dispersion)</summary>
        public GeneralMatrix Vcovariance {
            get {
                if (!seCalculated) {
                    CalculateStandardErrors();
                }
                return vcovariance;
            }
            private set { vcovariance = value; }
        }
        private GeneralMatrix vcovarianceFull;
        /// <summary>Variance-covariance matrix of all estimates (including dispersion)</summary>
        public GeneralMatrix VcovarianceFull {
            get {
                if (!seCalculated) {
                    CalculateStandardErrors();
                }
                return vcovarianceFull;
            }
            private set { vcovarianceFull = value; }
        }

        /// <summary>Linear predictor (transformed scale)</summary>
        public double[] LinearPredictor { get; private set; }

        private bool fittedValuesCalculated;
        private double[] fittedValues;
        /// <summary>Fitted values (response scale) integrated over the random effect.</summary>
        public double[] FittedValues {
            get {
                if (!fittedValuesCalculated) {
                    CalculateFittedValues();
                }
                return fittedValues;
            }
            private set { fittedValues = value; }
        }


        private double _LogLik = double.NaN;

        /// <summary>Log-likelihood.</summary>
        public double LogLik {
            get { return _LogLik; }
            set { _LogLik = value; }
        }

        //public double LogLik { get; private set; }
        /// <summary>Deviance (or -2*Log-Likelihood).</summary>
        public double _2LogLik { get { return -2.0 * LogLik; } }

        private int _evaluations = 0;
        /// <summary>Number of function evaluations.</summary>
        public int Evaluations {
            get { return _evaluations; }
            set { _evaluations = value; }
        }

        // public int Evaluations { get; private set; }
        /// <summary>Number of quadrature points in one-dimension for Gauss-Hermite quadrature</summary>
        public int GaussHermitePoints { get; private set; }


        private string _error = string.Empty;
        /// <summary>Error message; if empty no error.</summary>
        public string Error {
            get { return _error; }
            set { _error = value; }
        }

        //public string Error { get; private set; }

        /// <summary>Log-likelihood contribution for each individual</summary>
        //public double[] LogLikContribution { get; private set; }
        public List<double> LogLikContribution { get; private set; }

        private bool modelAssistedFrequencyCalculated;
        private double[] modelAssistedFrequency;
        /// <summary>Model assisted individually based frequency.</summary>
        public double[] ModelAssistedFrequency {
            get {
                if (!modelAssistedFrequencyCalculated) {
                    CalculateModellAssistedFrequency();
                }
                return modelAssistedFrequency;
            }
            private set { modelAssistedFrequency = value; }
        }

        /// <summary>Linear predictor for Predictions after a call to Predict.</summary>
        public double[] PredictLinearPredictor { get; private set; }
        /// <summary>Backtransformed, i.e. ILOGIT, Linear predictor for Predictions after a call to Predict.</summary>
        public double[] PredictLinearPredictorBackTransformed { get; private set; }
        /// <summary>Fitted values (on probability scale) for Predictions after a call to Predict. These are obtained by integrating out the random effect.</summary>
        public double[] PredictFittedValues { get; private set; }
        /// <summary>Standard errors of Linear predictor for Predictions after a call to Predict.</summary>
        public double[] PredictLinearPredictorSe { get; private set; }
        /// <summary>Standard errors of Fitted values (on probability scale) for Predictions after a call to Predict.</summary>
        public double[] PredictFittedValuesSe { get; private set; }

        /// <summary>To pass data for calculation of Log-Likelihood.</summary>
        private DataLogisticNormal dln;
        /// <summary>One dimensional Gauss Hermite integration points and weights.</summary>
        private double[,] ghXW;
        /// <summary>Number of units.</summary>
        private int nresponse;
        /// <summary>Number of regression parameters.</summary>
        private int npredictors;
        /// <summary>Saves the parameters for which the likelihood is calculated in the optimization process.</summary>
        private List<double[]> saveParameters;
        /// <summary>Saves the likelihood values in the optimization process.</summary>
        private List<double> saveLogLik;

        /// <summary>Initializes the Class with default number of Gauss-Hermite integration points = 32.</summary>
        public LogisticCalculator() {
            Initialize(32);
        }
        /// <summary>Initializes the Class with specification of the number of Gauss-Hermite integration points.</summary>
        /// <param name="gaussHermitePoints">Number of Gauss-Hermite integration points.</param>
        public LogisticCalculator(int gaussHermitePoints) {
            Initialize(gaussHermitePoints);
        }

        private void Initialize(int gaussHermitePoints) {
            GaussHermitePoints = gaussHermitePoints;
            MaxEvaluations = 1000;
            ToleranceAbsolute = 1.0e-6;
            ToleranceRelative = 1.0e-6;
            DispersionFix = double.NaN;
            SeMaxCycle = 2;
            ghXW = UtilityFunctions.GaussHermite(GaussHermitePoints);
            //setOutputNull();
            //setPredictNull();
        }

        //private void setOutputNull() {
        //    this.seCalculated = false;
        //    this.fittedValuesCalculated = false;
        //    this.modelAssistedFrequencyCalculated = false;
        //     this.Estimates = null;
        //    this.Dispersion = double.NaN;
        //    this.VcovarianceFull = null;
        //    this.LinearPredictor = null;
        //    this.LogLik = double.NaN;
        //    this.Evaluations = 0;
        //    this.Error = string.Empty;
        //    this.LogLikContribution = null;
        //    // The properties below use their local counterpart
        //    this.se = null;
        //    this.fittedValues = null;
        //    this.dispersionSe = double.NaN;
        //    this.vcovariance = null;
        //    this.modelAssistedFrequency = null;
        //}

        //private void setPredictNull() {
        //    PredictLinearPredictor = null;
        //    PredictLinearPredictorBackTransformed = null;
        //    PredictFittedValues = null;
        //    PredictLinearPredictorSe = null;
        //    PredictFittedValuesSe = null;
        //}

        /// <summary>
        /// Fit the Logistic Normal (LNN0) model.
        /// </summary>
        /// <param name="response">Binomial response.</param>
        /// <param name="nbinomial">Binomial totals.</param>
        /// <param name="designMatrix">Design matrix for regression.</param>
        /// <param name="individual">Individuals.</param>
        public void Fit(List<int> response, List<int> nbinomial, double[,] designMatrix, List<int> individual) {
            try {
                int nresponse = designMatrix.GetLength(0);
                var weights = Enumerable.Repeat(1D, nresponse).ToList();
                localFit(response, nbinomial, designMatrix, individual, weights);
            } catch (Exception ee) {
                string msg = "Logistic-Normal model (LNN0) aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Fit the Logistic Normal (LNN0) model with Weights.
        /// </summary>
        /// <param name="response">Binomial response.</param>
        /// <param name="nbinomial">Binomial totals.</param>
        /// <param name="designMatrix">Design matrix for regression.</param>
        /// <param name="individual">Individuals.</param>
        /// <param name="weights">Statistical weights (must be the same within individuals.</param>
        public void Fit(List<int> response, List<int> nbinomial, double[,] designMatrix, List<int> individual, List<double> weights) {
            try {
                localFit(response, nbinomial, designMatrix, individual, weights);
            } catch (Exception ee) {
                var msg = "Logistic-Normal model (LNN0) aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        private void localFit(List<int> response, List<int> nbinomial, double[,] designMatrix, List<int> individual, List<double> weights) {
            // Do some basic checking
            if ((response == null) || (nbinomial == null) || (designMatrix == null) || (individual == null)) {
                var msg = "Response, Nbinomial, DesignMatrix and Individual must be set in LNN0fit.";
                throw new Exception(msg);
            }
            nresponse = designMatrix.GetLength(0);
            npredictors = designMatrix.GetLength(1);
            if ((nresponse != nbinomial.Count) || (nresponse != designMatrix.GetLength(0)) || (nresponse != individual.Count) || (nresponse != weights.Count)) {
                var msg = "Response, Nbinomial DesignMatrix, Individual and Weights must have the same dimension in LNN0fit.";
                throw new Exception(msg);
            }

            // Check that individuals are sorted, also get the number of Days for each individual
            // Also check that Weight and DesignMatrix are the same within individuals
            var NobsPerIndividual = new int[nresponse];
            var prevWeight = weights[0];
            var prevDesignRow = new double[npredictors];
            for (int i = 0; i < npredictors; i++) {
                prevDesignRow[i] = designMatrix[0, i];
            }
            var prevIndividual = individual[0];
            var currentConsumer = individual[0];
            var countIndividual = 0;
            var nDays = 0;
            for (int i = 0; i < nresponse; i++) {
                if (individual[i] < prevIndividual) {
                    throw new Exception("Individuals must be sorted in LNN0fit");
                }
                prevIndividual = individual[i];
                // Count number of Days;
                if (individual[i] == currentConsumer) {
                    nDays++;
                    if (prevWeight != weights[i]) {
                        throw new Exception("Statistical Weights must be the same within individuals in LNN0fit.");
                    }
                    for (int j = 0; j < npredictors; j++) {
                        if (prevDesignRow[j] != designMatrix[i, j]) {
                            throw new Exception("DesignMatrix values must be the same within individuals in LNN0fit.");
                        }
                    }
                } else {
                    NobsPerIndividual[countIndividual] = nDays;
                    countIndividual++;
                    currentConsumer = individual[i];
                    nDays = 1;
                    prevWeight = weights[i];
                    for (int jj = 0; jj < npredictors; jj++) {
                        prevDesignRow[jj] = designMatrix[i, jj];
                    }
                }
            }
            NobsPerIndividual[countIndividual] = nDays;
            countIndividual++;
            LogLikContribution = new List<double>();

            // Fit Logistic Regression
            var lg = new LogisticRegression() {
                MaxCycle = 100,
                ToleranceAbsolute = 1e-6,
                ToleranceRelative = 1e-6,
                DispersionFix = 1D,
            };

            lg.Fit(response, nbinomial, designMatrix, weights);

            // Create Data object
            dln = new DataLogisticNormal() {
                Response = response,
                Nbinomial = nbinomial,
                DesignMatrix = designMatrix,
                CountIndividual = countIndividual,
                Individual = individual,
                NobsPerIndividual = NobsPerIndividual,
                Weights = weights,
                CurrentEstimates = lg.Estimates,
                DispersionFix = DispersionFix,
            };


            // Loop to find initial estimate for dispersion
            LinearPredictor = new double[nresponse];
            saveLogLik = new List<double>();
            saveParameters = new List<double[]>();
            if (double.IsNaN(DispersionFix)) {
                var mingrid = -4D;
                var maxgrid = 3D;
                var ngrid = 28;
                var incgrid = (maxgrid - mingrid) / ngrid;
                var disp = mingrid;
                var bestLogLik = double.MaxValue;
                var bestDisp = 0D;
                var LogLik1 = 0D;
                for (int i = 0; i < ngrid; i++) {
                    dln.CurrentDispersion = disp;
                    CalculateLogLik();
                    if (i == 0) {
                        LogLik1 = LogLik;
                    }
                    if (LogLik < bestLogLik) {
                        bestLogLik = LogLik;
                        bestDisp = disp;
                    }
                    disp += incgrid;
                }
                dln.CurrentDispersion = bestDisp;
            } else {
                dln.CurrentDispersion = Math.Log(DispersionFix);
            }

            // initial and estimates are used in the Simplex routine
            double[] initial;
            double[] estimates;
            if (double.IsNaN(DispersionFix)) { // Include dispersion parameter in estimation
                initial = new double[npredictors + 1];
                estimates = new double[npredictors + 1];
                lg.Estimates.CopyTo(initial, 0);
                initial[npredictors] = dln.CurrentDispersion;
            } else { // Exclude dispersion parameter in estimation
                initial = new double[npredictors];
                estimates = new double[npredictors];
                lg.Estimates.CopyTo(initial, 0);
            }


            // Optimize
            var Simplex = new OptimizeSimplex() {
                Tolerance = ToleranceRelative,
                MaxEvaluations = MaxEvaluations,
            };
            var fmin = Simplex.Minimize(initial, out estimates, CalculateLogLik, 1.0, 1);
            LogLik = fmin;
            Evaluations = Simplex.Evaluations;
            Estimates = new List<double>();
            for (int i = 0; i < npredictors; i++) {
                Estimates.Add(estimates[i]);
            }
            if (double.IsNaN(DispersionFix)) {
                Dispersion = Math.Exp(estimates[npredictors]);
            } else {
                Dispersion = DispersionFix;
            }
            LogLik = -1 * LogLik;
        }

        /// <summary>
        /// Forms predictions on the transformed logit scale (PredictLinearPredictor) and on the probability scale (PredictFittedValues).
        /// </summary>
        /// <param name="PredictionMatrix">Prediction matrix with covariate patterns for which to calculate prediction.</param>
        public void Predict(double[,] PredictionMatrix) {
            try {
                if (PredictionMatrix == null) {
                    var msg = "PredictionMatrix must be set in LogisticRegression.";
                    throw new Exception(msg);
                }
                if (Estimates == null) {
                    var msg = "No LNN0 regression model has been fitted. Prediction can not be formed.";
                    throw new Exception(msg);
                }
                int ncol = PredictionMatrix.GetLength(1);
                if (ncol != Estimates.Count) {
                    var msg = "The number of columns of PredictionMatrix must equal the number of estimates.";
                    throw new Exception(msg);
                }
                var factor = Math.Sqrt(2.0 * Dispersion);
                var ghx = new double[GaussHermitePoints];
                for (int k = 0; k < GaussHermitePoints; k++) {
                    ghx[k] = factor * ghXW[k, 0];
                }
                var npred = PredictionMatrix.GetLength(0);
                PredictLinearPredictor = new double[npred];
                PredictLinearPredictorBackTransformed = new double[npred];
                PredictFittedValues = new double[npred];
                PredictLinearPredictorSe = new double[npred];
                PredictFittedValuesSe = new double[npred];
                for (int i = 0; i < npred; i++) {
                    var lp = 0D;
                    var var = 0D;
                    for (int j = 0; j < ncol; j++) {
                        lp += Estimates[j] * PredictionMatrix[i, j];
                        for (int k = 0; k < ncol; k++) {
                            var += Vcovariance.GetElement(j, k) * PredictionMatrix[i, j] * PredictionMatrix[i, k];
                        }
                    }
                    var fit = 0.0;
                    var derivative = 0.0;
                    for (int iigh = 0; iigh < GaussHermitePoints; iigh++) {
                        double pp = UtilityFunctions.ILogit(lp + ghx[iigh]);
                        fit += ghXW[iigh, 1] * pp;
                        derivative += ghXW[iigh, 1] * pp * (1.0 - pp);
                    }
                    PredictLinearPredictor[i] = lp;
                    PredictLinearPredictorBackTransformed[i] = UtilityFunctions.ILogit(lp);
                    PredictFittedValues[i] = fit * invsqrtpi;
                    var = Math.Sqrt(var);
                    PredictLinearPredictorSe[i] = var;
                    PredictFittedValuesSe[i] = derivative * invsqrtpi * var;
                }
            } catch (Exception ee) {
                var msg = "Calculation of Predictions for the Logistic-Normal model (LNN)) aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        // This one is used by the Simplex algorithm
        private double CalculateLogLik(double[] args, object data) {
            var result = new List<double>();
            foreach (var item in args) {
                result.Add(item);
            }
            dln.CurrentEstimates = result;
            if (double.IsNaN(dln.DispersionFix)) {
                dln.CurrentDispersion = args.Last();
            }
            return CalculateLogLik();
        }

        // This one can be called outside the Simplex Algorithm
        private double CalculateLogLik() {
            var ghx = new double[GaussHermitePoints];
            var factor = Math.Sqrt(2.0 * UtilityFunctions.ExpBound(dln.CurrentDispersion));
            for (int k = 0; k < GaussHermitePoints; k++) {
                ghx[k] = factor * ghXW[k, 0];
            }
            var LogLik = 0D;

            var startIndiv = 0;
            var countZeroLik = 0;
            var maxLogLik = double.MinValue;
            for (int i = 0; i < dln.CountIndividual; i++) {
                var nDays = dln.NobsPerIndividual[i];
                // Linear predictor for current individual
                for (int j = 0; j < nDays; j++) {
                    var curRecord = startIndiv + j;
                    var lp = 0.0;
                    for (int kk = 0; kk < npredictors; kk++) {
                        lp += dln.CurrentEstimates[kk] * dln.DesignMatrix[curRecord, kk];
                    }
                    LinearPredictor[curRecord] = lp;
                }
                // Gauss Hermite integration for current individual
                var curLogLik = 0D;
                for (int iigh = 0; iigh < GaussHermitePoints; iigh++) {
                    var prob = 1D;
                    for (int jj = 0; jj < nDays; jj++) {
                        var curRecord = startIndiv + jj;
                        var pp = UtilityFunctions.ILogit(LinearPredictor[curRecord] + ghx[iigh]);
                        prob *= Math.Pow(pp, dln.Response[curRecord]) * Math.Pow(1.0 - pp, dln.Nbinomial[curRecord] - dln.Response[curRecord]);
                    }
                    curLogLik += ghXW[iigh, 1] * prob;

                }
                // Finalize log-likelihood
                if (curLogLik > 0) {
                    curLogLik = -dln.Weights[startIndiv] * Math.Log(curLogLik * invsqrtpi);
                    LogLikContribution.Add(curLogLik);
                    LogLik += curLogLik;
                    maxLogLik = Math.Max(maxLogLik, curLogLik);
                } else {
                    countZeroLik += 1;
                    LogLikContribution.Add(double.NaN);
                }
                startIndiv += nDays;
            }
            if (countZeroLik > 0) {
                LogLik += countZeroLik * maxLogLik;
            }
            // Save current Estimates and corresponding LogLik
            saveLogLik.Add(LogLik);

            if (double.IsNaN(dln.DispersionFix)) {
                var argscopy = dln.CurrentEstimates.ToList();
                argscopy.Add(dln.CurrentDispersion);
                saveParameters.Add(argscopy.ToArray());
            } else {
                saveParameters.Add(dln.CurrentEstimates.ToArray());
            }
            return LogLik;
        }

        private void CalculateFittedValues() {
            try {
                var factor = Math.Sqrt(2 * Dispersion);
                var ghx = new double[GaussHermitePoints];
                for (int k = 0; k < GaussHermitePoints; k++) {
                    ghx[k] = factor * ghXW[k, 0];
                }
                fittedValues = new double[nresponse];
                for (int jj = 0; jj < nresponse; jj++) {
                    var lp = LinearPredictor[jj];
                    var fit = 0D;
                    for (int iigh = 0; iigh < GaussHermitePoints; iigh++) {
                        fit += ghXW[iigh, 1] * UtilityFunctions.ILogit(lp + ghx[iigh]);
                    }
                    fittedValues[jj] = dln.Nbinomial[jj] * fit * invsqrtpi;
                }
                fittedValuesCalculated = true;
            } catch (Exception ee) {
                var msg = "Calculation of Fitted Values for the Logistic-Normal model (LNN)) aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        private void CalculateModellAssistedFrequency() {
            try {
                var factor = Math.Sqrt(2.0 * Dispersion);
                var ghx = new double[GaussHermitePoints];
                for (int k = 0; k < GaussHermitePoints; k++) {
                    ghx[k] = factor * ghXW[k, 0];
                }
                modelAssistedFrequency = new double[dln.CountIndividual];
                var curRecord = 0;
                for (int ii = 0; ii < dln.CountIndividual; ii++) {
                    var lp = LinearPredictor[curRecord];
                    // It is assumed that the covariate pattern within individuals is the same
                    // Otherwise it is not clear how to calculate a Model Assisted Frequency
                    // Loop to get the binomial response for the individual
                    var nDays = dln.NobsPerIndividual[ii];
                    var yyii = 0;
                    var nnii = 0;
                    for (int jj = 0; jj < nDays; jj++) {
                        yyii += dln.Response[curRecord];
                        nnii += dln.Nbinomial[curRecord];
                        curRecord++;
                    }
                    // Integrate over random effect
                    var denom = 0.0;
                    var nom = 0.0;
                    for (int iigh = 0; iigh < GaussHermitePoints; iigh++) {
                        var pp = UtilityFunctions.ILogit(lp + ghx[iigh]);
                        var pr = Math.Pow(pp, yyii) * Math.Pow(1.0 - pp, nnii - yyii);
                        denom += ghXW[iigh, 1] * pr;
                        nom += pp * ghXW[iigh, 1] * pr;
                    }
                    modelAssistedFrequency[ii] = nom / denom;
                }
                modelAssistedFrequencyCalculated = true;
            } catch (Exception ee) {
                var mes = "Calculation of Model Assisted Frequencies for the Logistic-Normal model (LNN)) aborted with the following exception:\n" + ee.Message;
                throw new Exception(mes);
            }
        }

        private void CalculateStandardErrors() {
            try {
                var parMin = new List<double>();
                if (double.IsNaN(DispersionFix)) {
                    parMin.AddRange(Estimates);
                    parMin.Add(Math.Log(Dispersion));
                } else {
                    parMin.AddRange(Estimates);
                }

                var logLikMin = -1.0 * LogLik;
                var stepderiv = stepSize2(saveLogLik, saveParameters, logLikMin, parMin.ToArray(), 2.0, 0.1);
                var MultiDeriv = new DerivativeMultiD {
                    MaxCycles = SeMaxCycle
                };
                double error;
                var var = MultiDeriv.Hessian(CalculateLogLik, parMin.ToArray(), ref logLikMin, stepderiv, out error);
                var varMat = new GeneralMatrix(var);
                try {
                    varMat = varMat.Inverse();
                } catch (Exception) {
                    // ToDo PGO: NCI Variance-covariance matrix is probably singular
                    throw;
                }

                vcovarianceFull = new GeneralMatrix(npredictors + 1, npredictors + 1);
                if (double.IsNaN(DispersionFix)) {
                    // Variance for dispersion parameter is on the Log-scale. 
                    var deriv = new double[npredictors + 1];
                    for (int i = 0; i < npredictors; i++) {
                        deriv[i] = 1D;
                    }
                    deriv[npredictors] = Dispersion;
                    var diagonal = GeneralMatrix.CreateDiagonal(deriv);
                    vcovarianceFull = diagonal * varMat.NanReplace(0.0) * diagonal;
                } else { 
                    // Add an extra column and row with NaN
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
                seCalculated = true;

                // Also set other structures
                se = new double[npredictors];
                for (int i = 0; i < npredictors; i++) {
                    se[i] = Math.Sqrt(vcovarianceFull.GetElement(i, i));
                }
                dispersionSe = Math.Sqrt(vcovarianceFull.GetElement(npredictors, npredictors));
                var rows = new List<int>();
                for (int i = 0; i < npredictors; i++) {
                    rows.Add(i);
                }
                vcovariance = vcovarianceFull.GetMatrix(rows.ToArray(), rows.ToArray());

                // Redo calculation of Loglikelihood and other save structures. This is necessary
                // because MultiDeriv.Hessian recalculates structures.
                dln.CurrentEstimates = Estimates;
                dln.CurrentDispersion = Dispersion;
                CalculateLogLik();
                Evaluations += MultiDeriv.Evaluations + 1;
            } catch (Exception ee) {
                var msg = "Calculation of the Variance-Covariance Matrix for the Logistic-Normal model (LNN)) aborted with the following exception:\n" + ee.Message;
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Estimates a stepsize for the calculation of a second-order derivative given a list 
        /// of function evaluations. This is done for each argument of the function.
        /// </summary>
        /// <param name="functionValues">Function values.</param>
        /// <param name="argValues">Function arguments.</param>
        /// <param name="function">Function value at arg.</param>
        /// <param name="arg">Function argument for which the second-order derivative must be calculated.</param>
        /// <param name="functionRange">Only function values F for which Abs(F - function) is smaller than functionRange are used.</param>
        /// <param name="functionChange">Change in function values which determines the stepsize.</param>
        /// <returns>Stepsize for each function argument.</returns>
        /// <remarks>Fits a linear regression Y = beta * X*X through the values Y=(functionValues - function) and X=(argValues - arg),
        /// using only Y values with an absolute value smaller than functionRange. 
        /// The stepsize which induces a change in the function value of size functionChange is then given by X = Sqrt(functionChange/Abs(beta)).
        /// </remarks>
        private static double[] stepSize2(
            List<double> functionValues,
            List<double[]> argValues,
            double function,
            double[] arg,
            double functionRange,
            double functionChange
        ) {
            int length = functionValues.Count;
            int nargs = argValues[0].Length;
            double[] xy = new double[nargs];
            double[] xx = new double[nargs];

            for (int i = 0; i < length; i++) {
                double diff = functionValues[i] - function;
                if (Math.Abs(diff) < functionRange) {
                    for (int j = 0; j < nargs; j++) {
                        double sqr = (argValues[i][j] - arg[j]).Squared();
                        xy[j] += sqr * diff;
                        xx[j] += sqr * sqr;
                    }
                }
            }
            for (int j = 0; j < nargs; j++) {
                var beta = Math.Abs(xy[j] / xx[j]);
                xy[j] = Math.Sqrt(functionChange / beta);
            }
            return xy;
        }
    }
}
