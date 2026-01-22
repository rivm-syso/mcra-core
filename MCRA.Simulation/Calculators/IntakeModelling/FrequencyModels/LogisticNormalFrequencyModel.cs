using log4net;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.Integrators;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class LogisticNormalFrequencyModel : FrequencyModel {

        private static readonly ILog log = LogManager.GetLogger(typeof(LogisticNormalFrequencyModel));
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

            log.Info("Logistic Normal frequency model is estimated");
            if (CovariateModel != CovariateModelType.Constant && CovariateModel != CovariateModelType.Cofactor) {
                if (TestingMethod == TestingMethodType.Backward) {
                    for (int dfPol = MaxDegreesOfFreedom; dfPol > MinDegreesOfFreedom - 1; dfPol--) {
                        frequencyDataResults = individualFrequencies.GetDMFrequency(CovariateModel, dfPol);
                        modelResults.Add(FitLogisticNormalModel(frequencyDataResults, isOneDay));
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
                        modelResults.Add(FitLogisticNormalModel(frequencyDataResults, isOneDay));
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
                    PValue = []
                };
            }

            frequencyDataResults = individualFrequencies.GetDMFrequency(CovariateModel, dfPolynomial);
            _modelResult = FitLogisticNormalModel(frequencyDataResults, isOneDay);

            log.Info("Estimate specified conditional predictions for logistic normal model");
            var specifiedPredictions = individualFrequencies.GetDMSpecifiedPredictions(CovariateModel, frequencyDataResults, predictionLevels);
            SpecifiedPredictions = GetPredictionLevels(specifiedPredictions);

            log.Info("Estimate conditional predictions for logistic normal model");
            var conditionalPredictions = individualFrequencies.GetDMConditionalPredictions(CovariateModel, frequencyDataResults);
            ConditionalPredictions = GetPredictionLevels(conditionalPredictions);

            log.Info("Estimate individual predictions for logistic normal model");
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
                    ParameterName = "dispersion",
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
        /// Get summary, but model estimation is skipped so set defaults
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
                    ParameterName = "dispersion (lower limit)",
                    Estimate = _modelResult.FrequencyModelDispersion,
                    StandardError = _modelResult.FrequencyModelDispersion,
                },
                FrequencyModelEstimates = [
                        new ParameterEstimates(){
                            ParameterName = errorMessage.GetDisplayName(),
                            Estimate = _modelResult.Estimates.First(),
                            StandardError = _modelResult.StandardErrors.First(),
                        }
                    ],
                _2LogLikelihood = _modelResult._2LogLikelihood,
                DegreesOfFreedom = _modelResult.DegreesOfFreedom,
                LikelihoodRatioTestResults = null,
                ErrorMessage = errorMessage,
            };
        }

        /// <summary>
        /// Fit the logistic normal distribution for frequencies
        /// </summary>
        /// <param name="fdr"></param>
        /// <returns>Results from a model fit</returns>
        private ModelResult FitLogisticNormalModel(FrequencyDataResult fdr, bool isOneDay) {
            var weights = fdr.Weights.ToList();
            var responses = fdr.X;
            var np = responses.GetLength(1);

            // Copy response and binomial totals to integer arrays.
            var ybin = fdr.Ybin.Select(Convert.ToInt32).ToList();
            var nbin = fdr.Nbin.Select(Convert.ToInt32).ToList();

            double? dispersionFix = null;
            if (isOneDay) {
                if (FixedDispersion < 0.0001) {
                    throw new Exception("Phi should be larger than 0.0001");
                } else if (double.IsNaN(FixedDispersion)) {
                    throw new Exception("Phi is undefined, specify value larger than 0.0001");
                }
                dispersionFix = FixedDispersion;
            }

            var calculator = new LogisticModelCalculator();
            var result = calculator.Compute(
                ybin,
                nbin,
                responses,
                weights,
                true,
                dispersionFix
            );

            return new ModelResult() {
                DegreesOfFreedom = Convert.ToInt32(result.DegreesOfFreedom),
                DfPolynomial = fdr.DfPolynomial,
                _2LogLikelihood = result.LogLikelihood * 2.0,
                FrequencyModelDispersion = result.Dispersion,
                DispersionSe = result.DispersionStandardError,
                Estimates = result.Estimates,
                StandardErrors = [.. result.StandardErrors],
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

            if (_modelResult.ErrorMessage == ErrorMessages.ModelIsSkipped0Frequencies
                || _modelResult.ErrorMessage == ErrorMessages.ModelIsSkipped100Frequencies
                || _modelResult.ErrorMessage == ErrorMessages.ModelIsSkippedEqualFrequencies
            ) {
                var result = new TransformFallback() { Probability = frequencyPrediction.prediction };
                return (result, frequencyPrediction.CovariateGroup);
            } else {
                var result = new LogisticNormalDistribution() {
                    Mu = UtilityFunctions.Logit(frequencyPrediction.prediction),
                    Sigma = Math.Sqrt(_modelResult.FrequencyModelDispersion),
                };
                return (result, frequencyPrediction.CovariateGroup);
            }
        }

        protected override ICollection<IndividualFrequency> CalculateModelAssistedFrequencies() {
            var factor = Math.Sqrt(2.0 * _modelResult.FrequencyModelDispersion);
            var ghx = new double[UtilityFunctions.GaussHermitePoints];

            var ghXW = UtilityFunctions.GaussHermite(UtilityFunctions.GaussHermitePoints);
            for (int k = 0; k < UtilityFunctions.GaussHermitePoints; k++) {
                ghx[k] = factor * ghXW[k, 0];
            }

            var lookup = new Dictionary<Tuple<double, int?, double>, double>();
            foreach (var item in _individualPredictions) {
                // Integrate over random effect
                var denom = 0.0;
                var nom = 0.0;
                var lookupKey = new Tuple<double, int?, double>(item.Frequency, item.Nbinomial, item.Prediction);
                if (!lookup.ContainsKey(lookupKey)) {
                    for (int i = 0; i < UtilityFunctions.GaussHermitePoints; i++) {
                        var pp = UtilityFunctions.ILogit(UtilityFunctions.Logit(item.Prediction) + ghx[i]);
                        var pr = Math.Pow(pp, (int)item.Frequency) * Math.Pow(1.0 - pp, (int)item.Nbinomial - (int)item.Frequency);
                        denom += ghXW[i, 1] * pr;
                        nom += pp * ghXW[i, 1] * pr;
                    }
                    lookup.Add(lookupKey, nom / denom);
                }

                item.ModelAssistedFrequency = lookup[lookupKey];
            }

            foreach (var item in _individualPredictions) {
                // Integrate over random effect
                var denom = 0.0;
                var nom = 0.0;
                for (int i = 0; i < UtilityFunctions.GaussHermitePoints; i++) {
                    var pp = UtilityFunctions.ILogit(UtilityFunctions.Logit(item.Prediction) + ghx[i]);
                    var pr = Math.Pow(pp, (int)item.Frequency) * Math.Pow(1.0 - pp, (int)item.Nbinomial - (int)item.Frequency);
                    denom += ghXW[i, 1] * pr;
                    nom += pp * ghXW[i, 1] * pr;
                }
                item.ModelAssistedFrequency = nom / denom;
            }
            return _individualPredictions;
        }

        private List<IndividualFrequency> GetPredictionLevels(FrequencyDataResult dm) {
            var results = new List<IndividualFrequency>();
            for (int i = 0; i < dm.X.GetLength(0); i++) {
                double prediction = 0;
                for (int j = 0; j < _modelResult.Estimates.Count; j++) {
                    prediction += dm.X[i, j] * _modelResult.Estimates[j];
                }
                results.Add(new IndividualFrequency(dm.SimulatedIndividuals?[i]) {
                    Prediction = UtilityFunctions.ILogit(prediction),
                    Cofactor = dm.Cofactor?[i],
                    Covariable = dm.Covariable?[i] ?? double.NaN,
                    NumberOfIndividuals = dm.GroupCounts?[i] ?? 0,
                    Nbinomial = (int)(dm.Nbin?[i] ?? 0),
                    Frequency = dm.Ybin?[i] ?? double.NaN
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
                        Prediction = c.Frequency,
                        Cofactor = c.Cofactor,
                        Covariable = c.Covariable,
                    })
                    .ToList(),
            };
        }
    }
}
