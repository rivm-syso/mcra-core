using log4net;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.NumericalRecipes;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class LNNModelCalculator {
        private static readonly ILog log = LogManager.GetLogger(typeof(LNNModelCalculator));

        public List<double> DailyIntakes { get; set; }
        public List<double> Weights { get; set; }
        public List<double> DailyIntakesTransformed { get; set; }

        public List<int> IndividualDays { get; set; }
        public double[,] FreqDesign { get; set; }
        public double[,] AmountDesign { get; set; }

        public LNNParameters LNNParameters { get; set; }

        /// <summary>
        /// Number of quadrature points in one-dimension for Gauss-Hermite quadrature
        /// </summary>
        public int GaussHermitePoints { get; set; }

        /// <summary>
        /// Criterion for pruning the quadrature points.
        /// </summary>
        public double GaussHermitePrune { get; set; }

        /// <summary>
        /// Maximum number of function evaluations in Simplex Routine; default 5000;
        /// </summary>
        public int MaxEvaluations { get; set; }

        /// <summary>
        /// Convergence criterion in minimization routines; default 1.0e-6.
        /// </summary>
        public double Tolerance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Evaluations { get; private set; }

        /// <summary>
        /// Maximum number of cycles for calculation of Standard Errors and Variance-covariance matrix.
        /// </summary>
        public int SeMaxCycle { get; set; }

        /// <summary>
        /// Which Standard Errors to return.
        /// </summary>
        public string SeReturn { get; set; }

        /// <summary>
        /// Estimates
        /// </summary>
        public LNNParameters Estimates { get; private set; }

        /// <summary>
        /// Standard errors of estimates.
        /// </summary>
        public LNNParameters Se { get; private set; }

        /// <summary>
        /// Variance-covariance matrix of estimates
        /// </summary>
        public GeneralMatrix Vcovariance { get; private set; }

        /// <summary>
        /// Variance-covariance matrix of transformed estimates  (equal to Vcovariance for Regression parameters)
        /// </summary>
        public GeneralMatrix VcovarianceT { get; private set; }

        /// <summary>
        /// Log-likelihood for model with initial estimates.
        /// </summary>
        public double LogLik0 { get; private set; }

        /// <summary>
        /// Deviance for model with initial estimates.
        /// </summary>
        public double Deviance0 { get { return 2.0 * LogLik; } }

        /// <summary>
        /// Log-likelihood 
        /// </summary>
        public double LogLik { get; private set; }

        /// <summary>
        /// Deviance 
        /// </summary>
        public double Deviance { get { return 2.0 * LogLik; } }

        /// <summary>
        /// Linear predictor for Frequency (transformed scale)
        /// </summary>
        public double[] FreqLp { get; set; }

        /// <summary>
        /// Linear predictor for Amounts (transformed scale);
        /// </summary>
        public double[] AmountLp { get; set; }

        /// <summary>
        /// Linear Predictor + Best Linear Unbiased Predictor for Individual effects (transformed scale)
        /// NaN for zero consumptions.
        /// </summary>
        public double[] AmountLpBlup { get; private set; }

        /// <summary>
        /// Linear Predictor + Best Linear Unbiased Predictor Modified for Individual effects (transformed scale)
        /// NaN for zero consumptions.
        /// </summary>
        public double[] AmountLpBlupModified { get; private set; }

        /// <summary>
        /// Best Linear Unbiased Predictor for Individual effects (transformed scale)
        /// NaN for non-consumers.
        /// </summary>
        public double[] AmountBlup { get; private set; }

        /// <summary>
        /// Best Linear Unbiased Predictor Modified for Individual effects (transformed scale)
        /// NaN for non-consumers.
        /// </summary>
        public double[] AmountBlupModified { get; private set; }

        /// <summary>
        /// Predictions for Frequency (on probability scale)
        /// </summary>
        public double[,] FreqPredictX { get; set; }

        /// <summary>
        /// Predictions for Frequency (on probability scale)
        /// </summary>
        public List<double> FreqPredict { get; set; }

        /// <summary>
        /// Predictions for Frequency (on transformed logistic scale)
        /// </summary>
        public List<double> FreqPredictLp { get; private set; }

        /// <summary>
        /// Predictions for Amount (on original scale)
        /// </summary>
        public List<double> AmountPredict { get; set; }

        /// <summary>
        /// Predictions for Amount (on transformed power scale)
        /// </summary>
        public List<double> AmountPredictLp { get; private set; }

        /// <summary>
        /// Predictions for Amount (on original scale)
        /// </summary>
        public double[,] AmountPredictX { get; set; }

        /// <summary>
        /// Whether to scale the amounts such that initial estimate of AmountVarBetween equals 1. 
        /// </summary>
        public bool ScaleAmountInAlgorithm { get; set; }

        /// <summary>
        /// Print  warning when convergence is not reached
        /// </summary>
        public ErrorMessages ErrorMessage { get; set; }

        /// <summary>
        /// Tow dimensional Gauss Hermite integration points and weights
        /// </summary>
        private double[,] ghXXW;

        /// <summary>
        /// Iteration
        /// </summary>
        private int Iteration;

        /// <summary>
        /// Saves the parameters for which the likelihood is calculated in the optimization process
        /// </summary>
        private List<double[]> SaveParameters;

        /// <summary>
        /// Saves the likelihood values in the optimization process.
        /// </summary>
        private List<double> SaveLogLik;

        /// <summary>
        /// Number of units for which the likelihood contribution is zero; this is indicative for an error during the optimization routine
        /// </summary>
        private int countZeroLik;

        /// <summary>
        /// Description (i.e. parameternames) of the rows and columns of Vcovariance
        /// </summary>
        private List<string> VcovarianceDescription;

        private IntakeTransformer intakeTransformer;

        /// <summary>
        /// Initializes the model calculator.
        /// </summary>
        public void Initialize() {
            ghXXW = SpecialFunctions.GaussHermiteTwoD(GaussHermitePoints, GaussHermitePrune);
        }

        public void Fit() {
            switch (LNNParameters.TransformType) {
                case TransformType.Logarithmic:
                    intakeTransformer = new LogTransformer();
                    break;
                case TransformType.NoTransform:
                    intakeTransformer = new IdentityTransformer();
                    break;
                case TransformType.Power:
                    intakeTransformer = new PowerTransformer() { Power = LNNParameters.Power };
                    break;
                default:
                    break;
            }

            log.Info("LNN model with correlation is fitted.");
            log.Info("Maximum number of function evaluations: " + MaxEvaluations.ToString() + " ; Tolerance: " + Tolerance.ToString());

            var Scaling = ScaleAmountInAlgorithm;    // Local copy of scaling
            Iteration = 0;
            Estimates = LNNParameters.Clone();

            TransformAmount();
            if (Scaling) {
                Scaling = DoScaling();
            }
            Estimates.Transform();
            var npar = 5 + Estimates.FreqEstimates.Count + Estimates.AmountEstimates.Count;
            //VcovarianceDescription = new string[npar];
            VcovarianceDescription = new List<string>();
            var parScale = new List<int>(); // 0 for no Scaling; 1 for Scaling with SqrtScale; 2 for Scaling with Scale
            var parList = new List<double>(); // Parameters which are estimated
            var parElement = new List<int>();
            FillParameters(parScale, parList, parElement);

            var initial = new List<double>();
            var estimate = new double[parList.Count];
            foreach (var item in parList) {
                initial.Add(item);
            }

            SaveLogLik = new List<double>();
            SaveParameters = new List<double[]>();

            if (parList.Count == 1) {
                var onedim = new OptimizeOneD() {
                    Tolerance = Tolerance,
                };
                var OneDeriv = new DerivativeOneD();
                var step = OneDeriv.StepSizeFixed(initial.First());
                var fmin = onedim.Minimize(initial.First(), step, CalculateLogLikSingle, out double xmin);
                Evaluations = onedim.Evaluations;
                var diffmin = 0D;
                if (Scaling) {
                    UnDoScaling();
                    var fminLocal = CalculateLogLik(Estimates);
                    diffmin = fmin - fminLocal;
                    LogLik0 -= diffmin;
                }
                Vcovariance = null;
                VcovarianceT = null;
            } else {
                var simplex = new OptimizeSimplex() {
                    Tolerance = Tolerance,
                    MaxEvaluations = MaxEvaluations,
                };

                //TODO dit kan sneller parallel van CalculateLogLik
                var fmin = simplex.Minimize(initial.ToArray(), out estimate, CalculateLogLik, 1D, 1);
                Evaluations = simplex.Evaluations;
                ErrorMessage = ErrorMessages.Convergence;
                if (simplex.Convergence == false) {
                    ErrorMessage = ErrorMessages.NoConvergence;
                    log.Info("Warning. The Simplex algorithm to fit the LNN model with correlation did not did not converge within " + MaxEvaluations.ToString() +
                        " function evaluations using " + "a tolerance of " + Tolerance.ToString() + ".");
                } else if (countZeroLik > 0) {
                    ErrorMessage = ErrorMessages.Error;
                    log.Info("Warning. Results for the LNN model with correlation are probably wrong because " + countZeroLik +
                        " observations have a Likelihood contribution of zero (and thus a Log-Likelihood contribution of minus infinity.");
                }
                var diffmin = 0D;
                if (Scaling) {
                    UnDoScaling();
                    var fminLocal = CalculateLogLik(Estimates);
                    diffmin = fmin - fminLocal;
                    LogLik0 -= diffmin;
                }
                Vcovariance = null;
                VcovarianceT = null;
            }
            calculatePredictions();
            //CalculateBlup();
            // Change signs of Log-Likelihoods
            LogLik0 = -LogLik0;
            LogLik = -LogLik;
        }

        /// <summary>
        /// Fill structures
        /// </summary>
        /// <param name="parScale"></param>
        /// <param name="parList"></param>
        /// <param name="parElement"></param>
        private void FillParameters(List<int> parScale, List<double> parList, List<int> parElement) {
            var iDescription = -1;
            for (int i = 0; i < Estimates.FreqEstimates.Count; i++) {
                iDescription++;
                VcovarianceDescription.Add("FrequencyBeta[" + i.ToString() + "]");
                if (Estimates.EstimateFrequency) {
                    parList.Add(Estimates.FreqEstimates[i]);
                    parScale.Add(0);
                    parElement.Add(iDescription);
                }
            }
            for (int i = 0; i < Estimates.AmountEstimates.Count; i++) {
                iDescription++;
                VcovarianceDescription.Add("AmountBeta[" + i.ToString() + "]");
                if (Estimates.EstimateAmount) {
                    parList.Add(Estimates.AmountEstimates[i]);
                    parScale.Add(1);
                    parElement.Add(iDescription);
                }
            }
            iDescription++;
            VcovarianceDescription.Add("FrequencyDispersionIndividuals");
            if (Estimates.EstimateDispersion) {
                parList.Add(Estimates.DispersionT);
                parScale.Add(0);
                parElement.Add(iDescription);
            }
            iDescription++;
            VcovarianceDescription.Add("AmountVarianceBetween");
            if (Estimates.EstimateVarianceBetween) {
                parList.Add(Estimates.VarianceBetweenT);
                parScale.Add(2);
                parElement.Add(iDescription);
            }
            iDescription++;
            VcovarianceDescription.Add("AmountVarianceWithin");
            if (Estimates.EstimateVarianceWithin) {
                parList.Add(Estimates.VarianceWithinT);
                parScale.Add(2);
                parElement.Add(iDescription);
            }
            iDescription++;
            VcovarianceDescription.Add("Correlation");
            if (Estimates.EstimateCorrelation) {
                parList.Add(Estimates.CorrelationT);
                parScale.Add(0);
                parElement.Add(iDescription);
            }
            iDescription++;
            VcovarianceDescription.Add("PowerTransformation");
            if (Estimates.EstimatePower) {
                parList.Add(Estimates.PowerT);
                parElement.Add(iDescription);
            }
        }

        private bool DoScaling() {
            // Scale DailyIntakes and Parameters such that the estimate for AmountVarBetween equals 1.0
            // Only when at least one of the Amount parameters is estimated
            if ((Estimates.EstimateAmount) || (Estimates.EstimateVarianceBetween) || (Estimates.EstimateVarianceWithin)) {
                var scale = 1D / LNNParameters.VarianceBetween;
                var sqrtScale = Math.Sqrt(scale);
                DailyIntakesTransformed = DailyIntakesTransformed.Select(c => c * sqrtScale).ToList();
                Estimates.AmountEstimates = Estimates.AmountEstimates.Select(c => c * sqrtScale).ToList();
                Estimates.VarianceBetween *= scale;
                Estimates.VarianceWithin *= scale;
                return true;
            } else {
                return false;
            }
        }

        private void UnDoScaling() {
            // Scale DailyIntakes and Parameters such that the estimate for AmountVarBetween equals 1.0
            // Only when at least one of the Amount parameters is estimated
            if ((Estimates.EstimateAmount) || (Estimates.EstimateVarianceBetween) || (Estimates.EstimateVarianceWithin)) {
                var scale = 1D / LNNParameters.VarianceBetween;
                var sqrtScale = Math.Sqrt(scale);
                DailyIntakesTransformed = DailyIntakesTransformed.Select(c => c / sqrtScale).ToList();
                Estimates.AmountEstimates = Estimates.AmountEstimates.Select(c => c / sqrtScale).ToList();
                Estimates.VarianceBetween /= scale;
                Estimates.VarianceWithin /= scale;
                Estimates.VarianceBetweenT -= Math.Log(scale);
                Estimates.VarianceWithinT -= Math.Log(scale);
            }
        }

        /// <summary>
        /// Transforms all DailyIntakes to DailyIntakesTransformed, changed 24-4-2013, see repository for original version about scale parameter
        /// </summary>
        /// 
        public void TransformAmount() {
            DailyIntakesTransformed = DailyIntakes.Select(c => (c > 0 ? intakeTransformer.Transform(c) : double.NaN)).ToList();
        }

        /// <summary>
        /// Back-transforms all DailyIntakeTransformed
        /// </summary>
        /// 
        public void TransformInverseAmount() {
            DailyIntakes = DailyIntakesTransformed.Select(c => (!double.IsNaN(c) ? intakeTransformer.InverseTransform(c) : 0)).ToList();
        }

        /// <summary>
        /// Back-transforms a single dailyIntakeTransformed
        /// </summary>
        /// <param name="dailyIntakeTransformed"></param>
        /// 
        /// <returns></returns>
        public double TransformInverseAmount(double dailyIntakeTransformed) {
            if (!double.IsNaN(dailyIntakeTransformed)) {
                return intakeTransformer.InverseTransform(dailyIntakeTransformed);
            }
            return 0D;
        }

        private void calculatePredictions() {
            FreqPredict = new List<double>();
            FreqPredictLp = new List<double>();
            for (int i = 0; i < FreqPredictX.GetLength(0); i++) {
                var predict = getPrediction(i, Estimates.FreqEstimates, FreqPredictX);
                FreqPredictLp.Add(predict);
                FreqPredict.Add(UtilityFunctions.ILogit(predict));
            }
            AmountPredict = new List<double>();
            AmountPredictLp = new List<double>();
            for (int i = 0; i < AmountPredictX.GetLength(0); i++) {
                var predict = getPrediction(i, Estimates.AmountEstimates, AmountPredictX);
                AmountPredictLp.Add(predict);
                AmountPredict.Add(TransformInverseAmount(predict));
            }
        }

        /// <summary>
        /// returns prediction
        /// </summary>
        /// <param name="i"></param>
        /// <param name="estimates"></param>
        /// <param name="predictionsX"></param>
        /// <returns></returns>
        private double getPrediction(int i, List<double> estimates, double[,] predictionsX) {
            var result = 0D;
            for (int j = 0; j < estimates.Count; j++) {
                result += estimates[j] * predictionsX[i, j];
            }
            return result;
        }

        private double CalculateLogLik(double[] args, object data) {
            var ipar = -1;
            if (Estimates.EstimateFrequency) {
                for (int i = 0; i < Estimates.FreqEstimates.Count; i++) {
                    ipar++;
                    Estimates.FreqEstimates[i] = args[ipar];
                }
            }
            if (Estimates.EstimateAmount) {
                for (int i = 0; i < Estimates.AmountEstimates.Count; i++) {
                    ipar++;
                    Estimates.AmountEstimates[i] = args[ipar];
                }
            }
            if (Estimates.EstimateDispersion) {
                ipar++;
                Estimates.DispersionT = args[ipar];
            }
            if (Estimates.EstimateVarianceBetween) {
                ipar++;
                Estimates.VarianceBetweenT = args[ipar];
            }
            if (Estimates.EstimateVarianceWithin) {
                ipar++;
                Estimates.VarianceWithinT = args[ipar];
            }
            if (Estimates.EstimateCorrelation) {
                ipar++;
                Estimates.CorrelationT = args[ipar];
            }
            if (Estimates.EstimatePower) {
                ipar++;
                Estimates.PowerT = args[ipar];
            }
            Estimates.InverseTransform();
            // Transform if necessary
            if (Estimates.EstimatePower) {
                TransformAmount();
            }
            // 
            LogLik = CalculateLogLik(Estimates);
            if (SaveLogLik != null) {
                SaveLogLik.Add(LogLik);
                // Create new double array; otherwise a reference is copied
                var argscopy = new List<double>();
                for (int i = 0; i < ipar + 1; i++) {
                    argscopy.Add(args[i]);
                }
                SaveParameters.Add(argscopy.ToArray());
            }
            return LogLik;
        }

        private double CalculateLogLikSingle(double arg, object data) {
            Estimates.DispersionT = Estimates.EstimateDispersion ? arg : Estimates.DispersionT;
            Estimates.VarianceBetweenT = Estimates.EstimateVarianceBetween ? arg : Estimates.VarianceBetweenT;
            Estimates.VarianceWithinT = Estimates.EstimateVarianceWithin ? arg : Estimates.VarianceWithinT;
            Estimates.CorrelationT = Estimates.EstimateCorrelation ? arg : Estimates.CorrelationT;
            Estimates.PowerT = Estimates.EstimatePower ? arg : Estimates.PowerT;
            Estimates.FreqEstimates[0] = Estimates.EstimateFrequency ? arg : Estimates.FreqEstimates[0];
            Estimates.AmountEstimates[0] = Estimates.EstimateAmount ? arg : Estimates.AmountEstimates[0];
            Estimates.InverseTransform();
            // Transform if necessary
            if (Estimates.EstimatePower) {
                TransformAmount();
            }
            LogLik = CalculateLogLik(Estimates);
            SaveLogLik.Add(LogLik);
            SaveParameters.Add(new double[] { arg });
            return LogLik;
        }

        private double CalculateLogLik(LNNParameters parameters) {
            var logLik = calculateLogLikContribution(parameters);
            if (Iteration == 0) {
                LogLik0 = logLik;
            }
            Iteration++;
            if (Iteration % 10 == 0) {
                log.Info(Iteration + " ");
            }
            return logLik;
        }

        private double calculateLogLikContribution(LNNParameters parameters) {
            // Back-transform parameters
            parameters.InverseTransform();

            // Create rotation parameters and scale Gauss Hermite Integration points
            var nPoints = ghXXW.GetLength(0);
            var aa = (Math.Sqrt(1 + parameters.Correlation) + Math.Sqrt(1 - parameters.Correlation)) / Math.Sqrt(2);
            var bb = (Math.Sqrt(1 + parameters.Correlation) - Math.Sqrt(1 - parameters.Correlation)) / Math.Sqrt(2);
            var aaX = new double[nPoints]; double[] bbX = new double[nPoints];
            for (int ii = 0; ii < nPoints; ii++) {
                aaX[ii] = aa * ghXXW[ii, 0] + bb * ghXXW[ii, 1];
                bbX[ii] = aa * ghXXW[ii, 1] + bb * ghXXW[ii, 0];
            }

            // Loop over Individuals to determine the likelihood contribution for each individual
            var sqrtFreqVarIndividuals = Math.Sqrt(parameters.FrequencyModelDispersion);
            var sqrtAmountVarBetween = Math.Sqrt(parameters.VarianceBetween);
            var sqrtAmountVarWithin = Math.Sqrt(parameters.VarianceWithin);
            var startIndiv = 0;
            countZeroLik = 0;
            var maxLogLik = double.MinValue;
            var logLikelihood = 0D;

            for (int i = 0; i < IndividualDays.Count; i++) {
                var nDays = IndividualDays[i];
                // Linear predictors for Frequency and Amount
                for (int jj = 0; jj < nDays; jj++) {
                    var curRecord = startIndiv + jj;
                    FreqLp[curRecord] = getContributionLP(parameters.FreqEstimates, FreqDesign, curRecord);
                    AmountLp[curRecord] = getContributionLP(parameters.AmountEstimates, AmountDesign, curRecord);
                }
                // Double Gauss Hermite Integration
                var curLogLik = 0D;

                for (int iigh = 0; iigh < nPoints; iigh++) {
                    var prob = 1D;
                    for (int j = 0; j < nDays; j++) {
                        var curRecord = startIndiv + j;
                        var pp = UtilityFunctions.ILogit(sqrtFreqVarIndividuals * aaX[iigh] + FreqLp[curRecord]);
                        if (double.IsNaN(DailyIntakesTransformed[curRecord])) {
                            prob *= (1 - pp);
                        } else {
                            var dAmount = DailyIntakesTransformed[curRecord];
                            var contrib = UtilityFunctions.PRNormal(sqrtAmountVarBetween * bbX[iigh] + AmountLp[curRecord] - dAmount, 0D, parameters.VarianceWithin, sqrtAmountVarWithin);
                            prob *= pp * contrib;
                        }
                    }
                    curLogLik += ghXXW[iigh, 2] * prob;
                }

                if (curLogLik > 0) {
                    curLogLik = -Math.Log(curLogLik / Math.PI) * Weights[startIndiv];
                    logLikelihood += curLogLik;
                    maxLogLik = Math.Max(maxLogLik, curLogLik);
                } else {
                    countZeroLik += 1;
                }
                startIndiv += nDays;
            }
            if (countZeroLik > 0) {
                logLikelihood += countZeroLik * maxLogLik;
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
        private double getContributionLP(List<double> estimates, double[,] design, int curRecord) {
            var result = 0D;
            for (int k = 0; k < design.GetLength(1); k++) {
                result += estimates[k] * design[curRecord, k];
            }
            return result;
        }
    }
}
