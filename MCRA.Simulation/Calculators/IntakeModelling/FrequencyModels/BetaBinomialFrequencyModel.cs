using log4net;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.Integrators;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Beta binomial frequency model
    /// </summary>
    public class BetaBinomialFrequencyModel : FrequencyModel {

        private static readonly ILog log = LogManager.GetLogger(typeof(BetaBinomialFrequencyModel));
        private ModelResult _modelResult;
        private List<IndividualFrequency> _individualPredictions;
        private ICollection<IndividualFrequency> _individualIntakeFrequencies;

        public override FrequencyModelSummary CalculateParameters(
            ICollection<IndividualFrequency> individualFrequencies,
            List<double> predictionLevels
        ) {
            _individualIntakeFrequencies = individualFrequencies;
            var isOneDay = individualFrequencies.Select(c => c.Nbinomial).Max() == 1;
            FrequencyDataResult frequencyDataResults = null;
            var modelResults = new List<ModelResult>();
            LikelihoodRatioTestResults LikelihoodRatioTest = null;

            var equalFrequencies = individualFrequencies.Select(c => c.Frequency / (double)c.Nbinomial).Variance() == 0;
            if (equalFrequencies) {
                return AllEqualFrequencyCalculator(individualFrequencies);
            }

            var empiricalFrequency = individualFrequencies.Sum(c => c.Frequency) / (double)individualFrequencies.Sum(c => c.Nbinomial);
            var incidentalIntakeType = GetIncidentalIntakeType(empiricalFrequency);
            if (incidentalIntakeType != IncidentalIntakeType.Incidental) {
                return SkippedModelFrequencyCalculator(incidentalIntakeType, individualFrequencies, empiricalFrequency);
            }

            log.Info("Beta Binomial frequency model is estimated");
            if (CovariateModel != CovariateModelType.Constant && CovariateModel != CovariateModelType.Cofactor) {
                if (TestingMethod == TestingMethodType.Backward) {
                    for (int dfPol = MaxDegreesOfFreedom; dfPol > MinDegreesOfFreedom - 1; dfPol--) {
                        frequencyDataResults = individualFrequencies.GetDMFrequency(CovariateModel, dfPol);
                        modelResults.Add(FitBetaBinomialModel(frequencyDataResults, isOneDay));
                        LikelihoodRatioTest = modelResults.GetLikelihoodRatioTest();
                        if (LikelihoodRatioTest != null) {
                            if (LikelihoodRatioTest.PValue.Last() < TestingLevel) {
                                LikelihoodRatioTest.IndexSelectedModel = LikelihoodRatioTest.PValue.Count - 1;
                                break;
                            } else {
                                if (dfPol == MinDegreesOfFreedom) {
                                    LikelihoodRatioTest.IndexSelectedModel = LikelihoodRatioTest.PValue.Count;
                                }
                            }
                        };
                    }
                } else {
                    for (int dfPol = MinDegreesOfFreedom; dfPol < MaxDegreesOfFreedom + 1; dfPol++) {
                        frequencyDataResults = individualFrequencies.GetDMFrequency(CovariateModel, dfPol);
                        modelResults.Add(FitBetaBinomialModel(frequencyDataResults, isOneDay));
                        LikelihoodRatioTest = modelResults.GetLikelihoodRatioTest();
                        if (LikelihoodRatioTest != null) {
                            if (LikelihoodRatioTest.PValue.Last() < TestingLevel) {
                                LikelihoodRatioTest.IndexSelectedModel = LikelihoodRatioTest.PValue.Count;
                                break;
                            } else {
                                if (dfPol == MaxDegreesOfFreedom) {
                                    LikelihoodRatioTest.IndexSelectedModel = 0;
                                }
                            }
                        }
                    }
                }
            }

            var dfPolynomial = MaxDegreesOfFreedom;
            if (LikelihoodRatioTest != null) {
                LikelihoodRatioTest.SelectedOrder = LikelihoodRatioTest.DfPolynomial[LikelihoodRatioTest.IndexSelectedModel];
                dfPolynomial = LikelihoodRatioTest.SelectedOrder;
            } else {
                LikelihoodRatioTest = new LikelihoodRatioTestResults() {
                    SelectedOrder = MaxDegreesOfFreedom,
                    PValue = new List<double>()
                };
            }

            frequencyDataResults = individualFrequencies.GetDMFrequency(CovariateModel, dfPolynomial);
            _modelResult = FitBetaBinomialModel(frequencyDataResults, isOneDay);

            log.Info("Estimate specified conditional predictions for Beta Binomial model");
            var specifiedPredictions = individualFrequencies.GetDMSpecifiedPredictions(CovariateModel, frequencyDataResults, predictionLevels);
            SpecifiedPredictions = GetPredictionLevels(specifiedPredictions);

            log.Info("Estimate conditional predictions for Beta Binomial model");
            var conditionalPredictions = individualFrequencies.GetDMConditionalPredictions(CovariateModel, frequencyDataResults);
            ConditionalPredictions = GetPredictionLevels(conditionalPredictions);

            log.Info("Estimate individual predictions for Beta Binomial model");
            var individualPredictions = individualFrequencies.GetDMIndividualPredictions(CovariateModel, frequencyDataResults);
            _individualPredictions = GetPredictionLevels(individualPredictions);

            var frequencyModelEstimates = new List<ParameterEstimates>();
            for (int i = 0; i < _modelResult.Estimates.Count; i++) {
                frequencyModelEstimates.Add(new ParameterEstimates() {
                    ParameterName = frequencyDataResults.DesignMatrixDescription[i],
                    Estimate = _modelResult.Estimates[i],
                    StandardError = _modelResult.StandardErrors[i],
                });
            }

            return new FrequencyModelSummary() {
                DispersionEstimates = new ParameterEstimates() {
                    ParameterName = "phi",
                    Estimate = _modelResult.FrequencyModelDispersion,
                    StandardError = _modelResult.DispersionSe,
                },
                FrequencyModelEstimates = frequencyModelEstimates,
                _2LogLikelihood = _modelResult._2LogLikelihood,
                DegreesOfFreedom = _modelResult.DegreesOfFreedom,
                LikelihoodRatioTestResults = LikelihoodRatioTest,
                ErrorMessage = ErrorMessages.Convergence,
            };
        }

        /// <summary>
        /// Get summary, but model estimation is skipped, so set defaults
        /// </summary>
        /// <param name="prediction"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public override FrequencyModelSummary GetDefaultModelSummary(ErrorMessages errorMessage) {
            _modelResult = GetDefaultModelResult(errorMessage);
            ConditionalPredictions = createDefaultConditionalPredictions(_individualIntakeFrequencies);
            SpecifiedPredictions = ConditionalPredictions;
            _individualPredictions = GetIndividualPredictions(_individualIntakeFrequencies);

            return new FrequencyModelSummary() {
                DispersionEstimates = new ParameterEstimates() {
                    ParameterName = "phi (lower limit)",
                    Estimate = _modelResult.FrequencyModelDispersion,
                    StandardError = _modelResult.FrequencyModelDispersion,
                },
                FrequencyModelEstimates = new List<ParameterEstimates>(){
                        new ParameterEstimates(){
                            ParameterName = errorMessage.GetDisplayName(),
                            Estimate = _modelResult.Estimates.First(),
                            StandardError = _modelResult.StandardErrors.First(),
                        }
                    },
                _2LogLikelihood = _modelResult._2LogLikelihood,
                DegreesOfFreedom = _modelResult.DegreesOfFreedom,
                LikelihoodRatioTestResults = null,
                ErrorMessage = errorMessage,
            };
        }

        /// <summary>
        /// Returns predictions for combination of covariates on the original scale
        /// (0, 1) and the number of counts.
        /// </summary>
        /// <param name="dm"></param>
        /// <returns></returns>
        private List<IndividualFrequency> GetPredictionLevels(FrequencyDataResult dm) {
            var results = new List<IndividualFrequency>();
            for (int i = 0; i < dm.X.GetLength(0); i++) {
                var prediction = 0D;
                for (int j = 0; j < _modelResult.Estimates.Count; j++) {
                    prediction += dm.X[i, j] * _modelResult.Estimates[j];
                }
                results.Add(new IndividualFrequency() {
                    Prediction = UtilityFunctions.ILogit(prediction),
                    Cofactor = dm.Cofactor?[i],
                    Covariable = dm.Covariable?[i] ?? double.NaN,
                    NumberOfIndividuals = dm.GroupCounts?[i] ?? 0,
                    Nbinomial = (int)(dm.Nbin?[i] ?? 0),
                    Frequency = dm.Ybin?[i] ?? double.NaN,
                    SimulatedIndividualId = dm.IdIndividual?[i] ?? 0,
                });
            }
            return results;
        }

        public override ConditionalPredictionResults GetConditionalPredictions() {
            return new ConditionalPredictionResults() {
                ConditionalPrediction = ConditionalPredictions
                    .Select(c => new ConditionalPrediction() {
                        Prediction = c.Prediction,
                        Cofactor = c.Cofactor,
                        Covariable = c.Covariable,
                    })
                    .ToList(),
                ConditionalData = _individualIntakeFrequencies
                    .Select(c => new ConditionalPrediction() {
                        Prediction = c.Frequency / Convert.ToDouble((int)c.Nbinomial),
                        Cofactor = c.Cofactor,
                        Covariable = c.Covariable,
                    })
                    .ToList(),
            };
        }

        public override (Distribution, CovariateGroup) GetDistribution(
            ICollection<IndividualFrequency> predictions,
            CovariateGroup targetCovariateGroup
        ) {
            var frequencyPrediction = predictions
                .Where(f => getCovariateGroup(targetCovariateGroup, f))
                .Select(f => (
                    CovariateGroup: new CovariateGroup {
                        Cofactor = f.Cofactor,
                        Covariable = f.Covariable,
                        NumberOfIndividuals = f.NumberOfIndividuals,
                    },
                    prediction: f.Prediction
                ))
                .Single();

            if (_modelResult.ErrorMessage == ErrorMessages.ModelIsSkipped0Frequencies || _modelResult.ErrorMessage == ErrorMessages.ModelIsSkipped100Frequencies || _modelResult.ErrorMessage == ErrorMessages.ModelIsSkippedEqualFrequencies) {
                var result = new TransformFallback() { Probability = frequencyPrediction.prediction };
                return (result, frequencyPrediction.CovariateGroup);
            } else {
                var result = new BetaDistribution(
                    shapeA: frequencyPrediction.prediction * (1 - _modelResult.FrequencyModelDispersion) / _modelResult.FrequencyModelDispersion,
                    shapeB: (1 - frequencyPrediction.prediction) * (1 - _modelResult.FrequencyModelDispersion) / _modelResult.FrequencyModelDispersion,
                    true
                );
                return (result, frequencyPrediction.CovariateGroup);
            }
        }

        protected override ICollection<IndividualFrequency> CalculateModelAssistedFrequencies() {
            foreach (var item in _individualPredictions) {
                var A = item.Prediction * (1 - _modelResult.FrequencyModelDispersion) / _modelResult.FrequencyModelDispersion;
                var B = (1 - item.Prediction) * (1 - _modelResult.FrequencyModelDispersion) / _modelResult.FrequencyModelDispersion;
                item.ModelAssistedFrequency = (double)((A + item.Frequency) / (A + B + item.Nbinomial));
            }
            return _individualPredictions;
        }

        /// <summary>
        /// IRLS algorithm for Beta Binomial distribution.
        /// </summary>
        /// <param name="fdr"></param>
        /// <param name="isOneDay"></param>
        /// <returns></returns>
        private ModelResult FitBetaBinomialModel(
            FrequencyDataResult fdr,
            bool isOneDay
        ) {
            List<double> regressionCoefficients;
            List<double> standardErrors;

            var noParameters = fdr.X.GetLength(1);
            var dfPol = fdr.DfPolynomial;
            var weight = fdr.Weights.ToArray();
            var nbin = fdr.Nbin.ToArray();
            var ybin = fdr.Ybin.ToArray();
            var x = fdr.X;
            var tol1 = 1e-4;
            var dispersion = 0D;
            var logitPhi = 0D;
            var ResDf = 0;
            var pearsonChi = 0D;
            var initial = false;
            var fMaxCycle = 10;

            //Put logic for datasets with one day for BBN here, cannot be generalized because LNN has different range for dispersion
            if (isOneDay) {

                if (FixedDispersion > 0.99) {
                    throw new Exception("FrequencyModelDispersion should be in the range (0.0001, 0.99)");
                } else if (FixedDispersion < 0.0001) {
                    throw new Exception("FrequencyModelDispersion should be in the range (0.0001, 0.99)");
                } else if (double.IsNaN(FixedDispersion)) {
                    throw new Exception("FrequencyModelDispersion is undefined, specify value within the range (0.0001, 0.99)");
                }

                dispersion = FixedDispersion;
                logitPhi = UtilityFunctions.Logit(dispersion);
                fMaxCycle = 1;
            }

            var n = ybin.Length;
            var Fit = new double[n];
            var fitp = new double[n];
            var lp = new double[n];

            if (!initial) {
                var nbin1 = new double[n];
                var localWeight = new double[n];
                var nobs = 0D;
                var meanNbin1 = 0D;

                for (int i = 0; i < n; i++) {
                    nbin1[i] = nbin[i] - 1;
                    nobs += weight[i];
                    meanNbin1 += (nbin1[i] - meanNbin1) / (i + 1);
                }

                for (int fmc = 0; fmc < fMaxCycle; fmc++) {
                    var temp = new double[n];
                    var loop = new double[11];
                    var check0 = 0D;
                    var check1 = 0D;
                    for (int i = 0; i < n; i++) {
                        localWeight[i] = weight[i] / (1 + dispersion * nbin1[i]);
                    }

                    LogisticRegression(x, localWeight, ybin, nbin, dfPol, Fit, fitp, out pearsonChi);

                    if (dispersion <= 0) {
                        dispersion = 1e-10;
                    }
                    //TODO
                    //Is bij weighted regression dit wel goed, moet het double zijn??
                    ResDf = Convert.ToInt32(nobs) - noParameters;

                    logitPhi = UtilityFunctions.Logit(dispersion);
                    dispersion = UtilityFunctions.ILogit(logitPhi);

                    if (Math.Abs(pearsonChi - ResDf) < (tol1 * ResDf)) {
                        break;
                    }

                    for (int i = 0; i < n; i++) {
                        temp[i] = weight[i] * Math.Pow((ybin[i] - Fit[i]), 2) / Fit[i] / (1 - Fit[i] / nbin[i]) / ResDf;
                    }
                    if (fmc == 0) {
                        loop[0] = (pearsonChi / ResDf - 1) / meanNbin1 + dispersion;
                    } else {
                        loop[0] = dispersion;
                    }
                    for (int j = 1; j < 11; j++) {
                        loop[j] = 0;
                        for (int i = 0; i < n; i++) {
                            if (weight[i] != 0) {
                                loop[j] += temp[i] / (1 / loop[j - 1] + nbin1[i]);
                            }
                        }
                    }
                    dispersion = loop[10];

                    for (int i = 0; i < n; i++) {
                        if (weight[i] != 0) {
                            check0 += temp[i] / (1 + loop[0] * nbin1[i]);
                            check1 += temp[i] / (1 + dispersion * nbin1[i]);
                            lp[i] = UtilityFunctions.Logit(fitp[i]);
                            //lp[i] = Utils.Bound(lp[i], liml, limr);
                        }
                    }

                    //No progress, alternative loop 
                    if (Math.Abs(check0 - 1) < Math.Abs(check1 - 1)) {
                        for (int j = 1; j < 11; j++) {
                            loop[j] = 0;
                            var sum = 0D;
                            for (int i = 0; i < n; i++) {
                                if (weight[i] != 0) {
                                    sum += temp[i] / (1 + loop[j - 1] * nbin1[i]);
                                }
                            }
                            loop[j] = loop[j - 1] / sum;
                        }
                        dispersion = loop[10];
                    }
                    if (dispersion > 1) {
                        dispersion = 1e-10;
                    }
                }
            } else {
                var initialEst = new double[n];
                logitPhi = UtilityFunctions.Logit(dispersion);
                dispersion = UtilityFunctions.ILogit(logitPhi);
                for (int i = 0; i < n; i++) {
                    fitp[i] = initialEst[i] / nbin[i];
                    lp[i] = UtilityFunctions.Logit(fitp[i]);
                }
            }

            if (isOneDay) {
                dispersion = FixedDispersion;
                logitPhi = UtilityFunctions.Logit(dispersion);
            }

            var fitpOld = fitp;
            var lpOld = lp;
            var phiOld = dispersion;
            var logLikOld = 0D;
            var _2LogLikelihood = 0D;
            var deltaPhi = 0D;
            var alfaLogLik = new double[n];
            var betaLogLik = new double[n];
            var d2LdxPhi2 = new double[n];
            var iW = new double[n];
            var score = new double[n];
            var yAdj = new double[n];
            var maxCycle = 30;

            var NCycle = 0;
            MultipleLinearRegressionResult mlrResults = null;
            for (int mc = 0; mc < maxCycle; mc++) {
                NCycle = mc + 1;
                NewtonRaphson(dispersion, out deltaPhi, fitp, weight, nbin, ybin, iW, score, d2LdxPhi2, alfaLogLik, betaLogLik);
                if (!isOneDay) {
                    logitPhi += deltaPhi;
                    dispersion = UtilityFunctions.ILogit(logitPhi);
                }
                for (int i = 0; i < n; i++) {
                    if (iW[i] != 0) {
                        yAdj[i] = lp[i] + score[i] / iW[i];
                    }
                }
                if (Function == FunctionType.Polynomial || dfPol < 2) {
                    mlrResults = MultipleLinearRegressionCalculator.Compute(x, yAdj.ToList(), iW.ToList());
                }

                _2LogLikelihood = LogLikelihood(nbin, ybin, weight, alfaLogLik, betaLogLik);
                var abs = new double[n];
                for (int i = 0; i < n; i++) {
                    fitp[i] = UtilityFunctions.ILogit(lp[i]);
                    abs[i] = Math.Abs(fitp[i] - fitpOld[i]) / fitp[i];
                }

                var critFit = abs.Max();
                var critLogLik = Math.Abs(_2LogLikelihood - logLikOld) / _2LogLikelihood;

                if (critFit <= tol1) {
                    break;
                }

                if (mc > 10) {
                    if (critLogLik <= (tol1 / 1e3)) {
                        break;
                    }
                    for (int i = 0; i < n; i++) {
                        fitp[i] = (fitp[i] + 2 * fitpOld[i]) / 3;
                        lp[i] = (lp[i] + 2 * lpOld[i]) / 3;
                    }
                    dispersion = (dispersion + 2 * phiOld) / 3;
                    logitPhi = UtilityFunctions.Logit(dispersion);
                }

                fitpOld = (double[])fitp.Clone();
                lpOld = (double[])lp.Clone();
                phiOld = dispersion;
                logLikOld = _2LogLikelihood;
            }


            ////Finalize analysis
            var variance = new double[n];
            var seLogitPhi = 0D;

            for (int i = 0; i < n; i++) {
                seLogitPhi += weight[i] * d2LdxPhi2[i];
            }

            _2LogLikelihood = LogLikelihood(nbin, ybin, weight, alfaLogLik, betaLogLik);

            seLogitPhi = -1 / seLogitPhi;
            if (seLogitPhi > 0) {
                seLogitPhi = Math.Sqrt(seLogitPhi);
            } else {
                seLogitPhi = 0;
            }

            var dispersionSe = seLogitPhi * dispersion * (1 - dispersion);

            ////Correct degrees of freedom for estimation of Phi.
            var degreesOfFreedom = Convert.ToInt32(ResDf) - 1;

            ////Get estimates for no covariable and nocofactor
            var meandev = 1D;
            if (Function == FunctionType.Polynomial) {
                standardErrors = mlrResults.StandardErrors.ToList();
                regressionCoefficients = mlrResults.RegressionCoefficients;
                meandev = mlrResults.MeanDeviance;
            } else {
                standardErrors = new double[x.GetLength(1)].ToList();
                regressionCoefficients = new List<double>();
            }

            //// The first element of FactorLevels is the reference in regression (dummy = 0)
            var coefficientConstant = regressionCoefficients[0];
            //TODO kijk dit nog eens na, heeft te maken met cofactor levels
            if (CovariateModel != CovariateModelType.Constant) {
                coefficientConstant = 0;
            }

            var bbpi = new double[regressionCoefficients.Count];
            var SeBBpi = new double[regressionCoefficients.Count];
            var tvaluebbpi = new double[regressionCoefficients.Count];
            var Tvalue = new double[regressionCoefficients.Count];
            for (int i = 0; i < regressionCoefficients.Count; i++) {
                bbpi[i] = regressionCoefficients[i] + coefficientConstant;
                standardErrors[i] = standardErrors[i] / Math.Sqrt(meandev);
                Tvalue[i] = regressionCoefficients[i] / standardErrors[i];
                bbpi[i] = UtilityFunctions.ILogit(bbpi[i]);
                SeBBpi[i] = standardErrors[i] * bbpi[i] * (1 - bbpi[i]);
                tvaluebbpi[i] = bbpi[i] / SeBBpi[i];
            }

            // This is a fix for the case when the number fitted coefficients is actually less than what was specified
            dfPol = regressionCoefficients.Count - noParameters + dfPol;

            return new ModelResult() {
                DegreesOfFreedom = degreesOfFreedom,
                DfPolynomial = dfPol,
                _2LogLikelihood = _2LogLikelihood,
                FrequencyModelDispersion = dispersion,
                DispersionSe = dispersionSe,
                Estimates = regressionCoefficients,
                StandardErrors = standardErrors,
                ErrorMessage = ErrorMessages.Convergence,
            };
        }

        /// <summary>
        /// Logistic weighted regression.
        /// </summary>
        /// <param name="x">Design matrix</param>
        /// <param name="weight">Weights</param>
        /// <param name="y">Binomial counts</param>
        /// <param name="nbin">Binomial totals</param>
        /// <param name="df">Degrees of freedom</param>
        /// <param name="fit"></param>
        /// <param name="fitp"></param>
        /// <param name="pearsonChi"></param>
        private void LogisticRegression(
            double[,] x,
            double[] weight,
            double[] y,
            double[] nbin,
            int df,
            double[] fit,
            double[] fitp,
            out double pearsonChi
        ) {
            var n = y.Length;
            var tol = 1e-5;
            var maxCycle = 50;
            var py = new double[n];
            var eta = new double[n];
            var z = new double[n];
            var w = new double[n];
            var nbweight = new double[n];
            double dev;
            var dev0 = 1e10;

            //Initialize structures
            for (int i = 0; i < n; i++) {
                py[i] = y[i] / nbin[i];
                fitp[i] = (0.1 + 0.8 * py[i]);
                eta[i] = UtilityFunctions.Logit(fitp[i]);
                nbweight[i] = weight[i] * nbin[i];
            }

            //Zeta gebruiken om individual exposure te benaderen
            //Weighted IRLS algorithm
            for (int mc = 0; mc < maxCycle; mc++) {
                var sum1 = 0D;
                var sum2 = 0D;
                for (int i = 0; i < n; i++) {
                    z[i] = eta[i] + (py[i] - fitp[i]) / (fitp[i] * (1 - fitp[i]));
                    w[i] = nbweight[i] * (fitp[i] * (1 - fitp[i]));
                }
                if (Function == FunctionType.Polynomial || df < 2) {
                    var mlrResults = MultipleLinearRegressionCalculator.Compute(x, z.ToList(), w.ToList());
                    eta = mlrResults.FittedValues.ToArray();
                }

                for (int i = 0; i < n; i++) {
                    if (weight[i] != 0) {
                        fitp[i] = UtilityFunctions.ILogit(eta[i]);
                        if (py[i] != 0 && py[i] != 1) {
                            sum1 += 2 * nbweight[i] * (py[i] * UtilityFunctions.LogBound(py[i]) + (1 - py[i]) * UtilityFunctions.LogBound(1 - py[i]));
                        }
                        sum2 += 2 * nbweight[i] * (py[i] * UtilityFunctions.LogBound(fitp[i]) + (1 - py[i]) * UtilityFunctions.LogBound(1 - fitp[i]));
                        if (fitp[i] == 1) {
                            fitp[i] = fitp[i] * (1 - 1e-14);
                        }
                    }
                }
                dev = sum1 - sum2;
                if (Math.Abs(dev0 - dev) < tol) {
                    break;
                } else {
                    dev0 = dev;
                }
            }

            pearsonChi = 0;
            for (int i = 0; i < n; i++) {
                if (weight[i] != 0 & w[i] != 0) {
                    pearsonChi += Math.Pow((y[i] * weight[i] - fitp[i] * weight[i] * nbin[i]), 2) / w[i];
                }
                fit[i] = nbin[i] * fitp[i];
            }
        }

        /// <summary>
        /// Calculates the -2*Loglikelihood.
        /// </summary>
        /// <param name="nbin">Binomial totals</param>
        /// <param name="ybin">Binomial counts</param>
        /// <param name="weight">Weights</param>
        /// <param name="alfa">Parameter alfa of Beta distribution</param>
        /// <param name="beta">Parameter beta of Beta distribution</param>
        /// <returns>Loglikelihood</returns>
        double LogLikelihood(double[] nbin, double[] ybin, double[] weight, double[] alfa, double[] beta) {
            var n = nbin.Length;
            var oi0 = new double[n];
            for (int i = 0; i < n; i++) {
                oi0[i] = SpecialFunctions.LnGamma(nbin[i] + 1) - SpecialFunctions.LnGamma(ybin[i] + 1) - SpecialFunctions.LnGamma(nbin[i] - ybin[i] + 1);
            }

            var oiSum = new double[n];
            var _2LogL = 0D;

            int[] d1 = { 1, -1, 1, -1, 1, -1 };
            int[] d2 = { 0, 1, 0, 0, 1, 0 };
            int[] d3 = { 1, 0, 0, 0, -1, 0 };
            int[] d4 = { 1, 1, 1, 1, 0, 0 };
            int[] d5 = { 0, 1, 1, 0, 1, 1 };
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < 6; j++) {
                    oiSum[i] += d1[j] * SpecialFunctions.LnGamma(d2[j] * nbin[i] + d3[j] * ybin[i] + d4[j] * alfa[i] + d5[j] * beta[i]);
                }
                oiSum[i] += oi0[i];
                _2LogL += -2 * oiSum[i] * weight[i];
            }
            return _2LogL;
        }

        /// <summary>
        /// Newton-Raphson algorithm.
        /// </summary>
        /// <param name="Phi">FrequencyModelDispersion parameter phi</param>
        /// <param name="deltaPhi">Change in phi</param>
        /// <param name="Fitp">Fitted values</param>
        /// <param name="weight">Weights</param>
        /// <param name="nbin">Binomial counts</param>
        /// <param name="ybin">Binomial totals</param>
        /// <param name="iW">Adapted weights</param>
        /// <param name="score">Score</param>
        /// <param name="d2LdxPhi2">Derivative of likelihood</param>
        /// <param name="alfa">Parameter alfa of Beta distribution</param>
        /// <param name="beta">Parameter beta of Beta distribution</param>
        void NewtonRaphson(
            double Phi,
            out double deltaPhi,
            double[] Fitp,
            double[] weight,
            double[] nbin,
            double[] ybin,
            double[] iW,
            double[] score,
            double[] d2LdxPhi2,
            double[] alfa,
            double[] beta
        ) {
            var n = ybin.Length;
            var dAdPhi = new double[n];
            var dBdPhi = new double[n];
            var zeta = new double[n];
            var eta = new double[n];
            var individualIntakeFreq = new double[n];
            for (int i = 0; i < n; i++) {
                alfa[i] = Fitp[i] * (1 - Phi) / Phi;
                beta[i] = (1 - Fitp[i]) * (1 - Phi) / Phi;
                dAdPhi[i] = -Fitp[i] / Phi / Phi;
                dBdPhi[i] = (-1 + Fitp[i]) / Phi / Phi;
                eta[i] = UtilityFunctions.Logit(Fitp[i]);
                zeta[i] = UtilityFunctions.Logit(Fitp[i]) + (ybin[i] / nbin[i] - Fitp[i]) / (Fitp[i] * (1 - Fitp[i]));
                individualIntakeFreq[i] = UtilityFunctions.ILogit(zeta[i]);
            }

            double dAdP, dBdP;
            dAdP = (1 - Phi) / Phi;
            dBdP = (-1 + Phi) / Phi;
            var diG = new double[n, 6];
            var triG = new double[n, 6];
            int[] c1 = { 0, 1, 0, 0, 1, 0 };
            int[] c2 = { 1, 0, 0, 0, -1, 0 };
            int[] c3 = { 1, 1, 1, 1, 0, 0 };
            int[] c4 = { 0, 1, 1, 0, 1, 1 };
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < 6; j++) {
                    diG[i, j] = SpecialFunctions.DiGamma(c1[j] * nbin[i] + c2[j] * ybin[i] + c3[j] * alfa[i] + c4[j] * beta[i]);
                    triG[i, j] = SpecialFunctions.TriGamma(c1[j] * nbin[i] + c2[j] * ybin[i] + c3[j] * alfa[i] + c4[j] * beta[i]);
                }
            }

            var dLdA = new double[n];
            var dLdB = new double[n];
            var d2LdA2 = new double[n];
            var d2LdB2 = new double[n];
            var d2LdAB = new double[n];
            var dLdPhi = new double[n];
            var d2LdPhi2 = new double[n];
            var dLdxPhi = new double[n];
            var dLdP = new double[n];
            var dPdLP = new double[n];
            var d2LdP2 = new double[n];
            var sumW1 = 0D;
            var sumW2 = 0D;
            for (int i = 0; i < n; i++) {
                dLdA[i] = diG[i, 0] - diG[i, 1] + diG[i, 2] - diG[i, 3];
                dLdB[i] = diG[i, 4] - diG[i, 1] + diG[i, 2] - diG[i, 5];
                d2LdA2[i] = triG[i, 0] - triG[i, 1] + triG[i, 2] - triG[i, 3];
                d2LdB2[i] = triG[i, 4] - triG[i, 1] + triG[i, 2] - triG[i, 5];
                d2LdAB[i] = -triG[i, 1] + triG[i, 2];
                dLdPhi[i] = dLdA[i] * dAdPhi[i] + dLdB[i] * dBdPhi[i];
                d2LdPhi2[i] = d2LdA2[i] * dAdPhi[i] * dAdPhi[i] + 2 * d2LdAB[i] * dAdPhi[i] * dBdPhi[i] + d2LdB2[i] * dBdPhi[i] * dBdPhi[i];
                dLdxPhi[i] = dLdPhi[i] * Phi * (1 - Phi);
                d2LdxPhi2[i] = (d2LdPhi2[i] * Phi * (1 - Phi) + dLdPhi[i] * (1 - 2 * Phi)) * Phi * (1 - Phi);
                sumW1 += weight[i] * dLdxPhi[i];
                sumW2 += weight[i] * d2LdxPhi2[i];
                dLdP[i] = dLdA[i] * dAdP + dLdB[i] * dBdP;
                dPdLP[i] = Fitp[i] * (1 - Fitp[i]);
                d2LdP2[i] = d2LdA2[i] * dAdP * dAdP + 2 * d2LdAB[i] * dAdP * dBdP + d2LdB2[i] * dBdP * dBdP;
                score[i] = weight[i] * dLdP[i] * dPdLP[i];
                iW[i] = -(weight[i] * d2LdP2[i] * dPdLP[i] * dPdLP[i]);
                if (iW[i] < 0) {
                    iW[i] = 0;
                }
            }
            deltaPhi = -sumW1 / sumW2;
        }
    }
}
