using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Utils;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Logistic normal normal model including correlation (known as NCI model) for chronic exposure assessment
    /// </summary>
    /// <param name="frequencyModelSettings"></param>
    /// <param name="amountModelSettings"></param>
    /// <param name="predictionLevels"></param>
    /// <param name="varianceRatio"></param>
    /// <param name="fixedDispersion"></param>
    public class LNNModel(
        IIntakeModelCalculationSettings frequencyModelSettings,
        IIntakeModelCalculationSettings amountModelSettings,
        List<double> predictionLevels,
        double varianceRatio = 1,
        double fixedDispersion = 0.0001
    ) : IntakeModel, IUncorrelatedIntakeModel {

        private List<ModelledIndividualAmount> _conditionalAmountsPredictions;
        private List<ModelledIndividualAmount> _specifiedAmountsPredictions;
        private List<IndividualFrequency> _conditionalFrequencyPredictions;
        private List<IndividualFrequency> _specifiedFrequencyPredictions;
        private LNN0Model _lnn0Model;
        private LNNParameters _estimates;
        private double[,] _vcov;

        public TransformType TransformType { get; set; }
        public IIntakeModelCalculationSettings AmountModelSettings { get; set; } = amountModelSettings;
        public IIntakeModelCalculationSettings FrequencyModelSettings { get; set; } = frequencyModelSettings;
        public int NumberOfMonteCarloIterations { get; set; }
        public FrequencyModelSummary FrequencyInitials { get; set; }
        public NormalAmountsModelSummary AmountInitials { get; set; }
        public FrequencyAmountModelSummary FrequencyAmountModelSummary { get; set; }
        public LNN0Model Lnn0Model { get; set; }
        public IntakeModelType FallBackModel { get; set; }
        public double VarianceRatio { get; set; } = varianceRatio;
        public double FixedDispersion { get; set; } = fixedDispersion;

        /// <summary>
        /// The exposure model type.
        /// </summary>
        public override IntakeModelType IntakeModelType {
            get {
                return IntakeModelType.LNN;
            }
        }

        public override void CalculateParameters(
            ICollection<SimpleIndividualDayIntake> individualDayIntakes
        ) {
            var covariateGroupCalculator = new CovariateGroupCalculator(
                predictionLevels,
                FrequencyModelSettings.CovariateModelType,
                AmountModelSettings.CovariateModelType
            );

            DataBasedCovariateGroups = covariateGroupCalculator
                .ComputeDataBasedCovariateGroups(individualDayIntakes);

            SpecifiedPredictionCovariateGroups = covariateGroupCalculator
                .ComputeSpecifiedPredictionsCovariateGroups(individualDayIntakes);

            _lnn0Model = GetLnn0Model();
            //Fit logistic normal model and amounts model
            _lnn0Model.CalculateParameters(individualDayIntakes);

            FrequencyInitials = _lnn0Model.FrequencyModelSummary;
            AmountInitials = (NormalAmountsModelSummary)_lnn0Model.AmountsModelSummary;

            var power = double.NaN;
            if (!double.IsNaN(FrequencyInitials._2LogLikelihood)) {
                FallBackModel = IntakeModelType.LNN;
                switch (TransformType) {
                    case TransformType.NoTransform:
                        power = 1;
                        break;
                    case TransformType.Logarithmic:
                        power = 0;
                        break;
                    case TransformType.Power:
                        power = ((PowerTransformer)AmountInitials.IntakeTransformer).Power;
                        break;
                }

                (_estimates, var stdErrors, var logLik, var errorMessage) = computeLNNModel(
                    individualDayIntakes,
                    power
                );

                designsLNNModel(
                    individualDayIntakes,
                    _estimates
                );

                var dispersion = new ParameterEstimates() {
                    ParameterName = "FrequencyModelDispersion",
                    Estimate = _estimates.Parameters.Dispersion,
                    StandardError = stdErrors.Parameters.Dispersion,
                };
                var correlation = new ParameterEstimates() {
                    ParameterName = "Correlation",
                    Estimate = _estimates.Parameters.Correlation,
                    StandardError = stdErrors.Parameters.Correlation,
                };

                var varianceBetween = new ParameterEstimates() {
                    ParameterName = "Variance between individuals",
                    Estimate = _estimates.Parameters.VarianceBetween,
                    StandardError = stdErrors.Parameters.VarianceBetween,
                };
                var varianceWithin = new ParameterEstimates() {
                    ParameterName = "Variance within individuals",
                    Estimate = _estimates.Parameters.VarianceWithin,
                    StandardError = stdErrors.Parameters.VarianceWithin,
                };

                // Set parameters of intakeFrequency to NCI parameters
                var parameterNames = FrequencyInitials.FrequencyModelEstimates.Select(c => c.ParameterName).ToList();
                var frequencyModelEstimates = new List<ParameterEstimates>();
                for (int i = 0; i < _estimates.FreqEstimates.Count; i++) {
                    frequencyModelEstimates.Add(new ParameterEstimates() {
                        ParameterName = parameterNames[i],
                        Estimate = _estimates.FreqEstimates[i],
                        StandardError = stdErrors.FreqEstimates[i]
                    });
                }
                var amountModelEstimates = new List<ParameterEstimates>();
                parameterNames = [.. AmountInitials.AmountModelEstimates.Select(c => c.ParameterName)];
                for (int i = 0; i < _estimates.AmountEstimates.Count; i++) {
                    amountModelEstimates.Add(new ParameterEstimates() {
                        ParameterName = parameterNames[i],
                        Estimate = _estimates.AmountEstimates[i],
                        StandardError = stdErrors.AmountEstimates[i]
                    });
                }

                _vcov = computeVcovMatrix(
                    dispersion.Estimate,
                    varianceBetween.Estimate,
                    correlation.Estimate
                );

                FrequencyAmountModelSummary = new FrequencyAmountModelSummary() {
                    CorrelationEstimates = correlation,
                    VarianceBetween = varianceBetween,
                    VarianceWithin = varianceWithin,
                    DispersionEstimates = dispersion,
                    FrequencyModelEstimates = frequencyModelEstimates,
                    AmountModelEstimates = amountModelEstimates,
                    DegreesOfFreedomFrequencies = FrequencyInitials.DegreesOfFreedom,
                    DegreesOfFreedomAmounts = AmountInitials.DegreesOfFreedom,
                    _2LogLikelihood = logLik,
                    Power = power,
                    ErrorMessage = errorMessage,
                };
            } else {
                // LNN with correlation cannot be fitted because exposure is not incidental.
                // Fit BNN or LNN without correlation instead
                Lnn0Model = _lnn0Model;
                FallBackModel = IntakeModelType.LNN0;
            }
        }

        public override List<ConditionalUsualIntake> GetConditionalIntakes(
            int seed,
            CompositeProgressState progressState = null
        ) {
            if (FallBackModel == IntakeModelType.LNN0) {
                return [.. _lnn0Model.GetConditionalIntakes(seed, progressState)
                    .OrderBy(c => c.CovariatesCollection.OverallCofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(c => c.CovariatesCollection.OverallCovariable)];
            }
            var parameters = _estimates.Parameters;
            var cancelToken = progressState?.CancellationToken ?? new();
            var predictionCovariateGroups = SpecifiedPredictionCovariateGroups;
            var results = predictionCovariateGroups
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(covariateGroup => {
                    var random = new McraRandomGenerator(RandomUtils.CreateSeed(seed, covariateGroup.GetHashCode()));
                    var freqMean = GetSpecifiedFrequencyPrediction(covariateGroup, out CovariateGroup fcg);
                    var amountMean = GetSpecifiedAmountPrediction(covariateGroup, out CovariateGroup acg);
                    var usualIntakes = new List<double>();
                    var mean = new List<double> { freqMean, amountMean };

                    var distribution = new MultiVariateNormalDistribution(mean, _vcov);
                    var randomSequence = distribution.Samples(random, NumberOfMonteCarloIterations);
                    for (int i = 0; i < randomSequence.Count; i++) {
                        var f = UtilityFunctions.ILogit(randomSequence[i][0]);
                        var a = AmountInitials.IntakeTransformer.BiasCorrectedInverseTransform(randomSequence[i][1], parameters.VarianceWithin);
                        usualIntakes.Add(a * f);
                    }
                    return new ConditionalUsualIntake() {
                        CovariatesCollection = new CovariatesCollection() {
                            FrequencyCofactor = fcg.Cofactor ?? string.Empty,
                            FrequencyCovariable = fcg.Covariable,
                            AmountCofactor = acg.Cofactor ?? string.Empty,
                            AmountCovariable = acg.Covariable,
                        },
                        CovariateGroup = covariateGroup,
                        ConditionalUsualIntakes = usualIntakes,
                    };
                })
                .ToList();
            if (results.Select(c => c.CovariatesCollection.AmountCofactor).First() != string.Empty) {
                return [.. results.OrderBy(c => c.CovariatesCollection.AmountCofactor, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(c => c.CovariatesCollection.AmountCovariable)
                        .ThenBy(c => c.CovariatesCollection.FrequencyCovariable)];
            } else if (results.Select(c => c.CovariatesCollection.FrequencyCofactor).First() != string.Empty) {
                return [.. results.OrderBy(c => c.CovariatesCollection.FrequencyCofactor, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(c => c.CovariatesCollection.FrequencyCovariable)
                        .ThenBy(c => c.CovariatesCollection.AmountCovariable)];
            } else {
                return [.. results.OrderBy(c => c.CovariatesCollection.FrequencyCovariable).ThenBy(c => c.CovariatesCollection.AmountCovariable)];
            }
        }

        public override List<ModelBasedIntakeResult> GetMarginalIntakes(
            int seed,
            CompositeProgressState progressState = null
        ) {
            if (FallBackModel == IntakeModelType.LNN0) {
                return _lnn0Model.GetMarginalIntakes(seed);
            }
            var parameters = _estimates.Parameters;
            var covariateGroups = DataBasedCovariateGroups;
            var n = NumberOfMonteCarloIterations / covariateGroups.Sum(c => c.NumberOfIndividuals);
            var numberOfIterationsPerIndividual = n > 0 ? n : 1;
            var cancelToken = progressState?.CancellationToken ?? new();
            return covariateGroups
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(covariateGroup => {
                    var random = new McraRandomGenerator(RandomUtils.CreateSeed(seed, covariateGroup.GetHashCode()));
                    var freqMean = GetConditionalFrequencyPrediction(covariateGroup, out CovariateGroup fcg);
                    var amountMean = GetConditionalAmountPrediction(covariateGroup, out CovariateGroup acg);
                    var usualIntakes = new List<double>();
                    var mean = new List<double> { freqMean, amountMean };

                    var distribution = new MultiVariateNormalDistribution(mean, _vcov);
                    var randomSequence = distribution.Samples(random, covariateGroup.NumberOfIndividuals * numberOfIterationsPerIndividual);
                    for (int i = 0; i < randomSequence.Count; i++) {
                        var f = UtilityFunctions.ILogit(randomSequence[i][0]);
                        var a = AmountInitials.IntakeTransformer.BiasCorrectedInverseTransform(randomSequence[i][1], parameters.VarianceWithin);
                        usualIntakes.Add(a * f);
                    }
                    return new ModelBasedIntakeResult() {
                        CovariateGroup = covariateGroup,
                        ModelBasedIntakes = usualIntakes,
                    };
                })
                .ToList();
        }

        public override List<ModelAssistedIntake> GetIndividualIntakes(int seed) {
            if (FallBackModel == IntakeModelType.LNN0) {
                return _lnn0Model.GetIndividualIntakes(seed);
            }
            return null;
        }

        public LNN0Model GetLnn0Model() {
            return new LNN0Model(
                FrequencyModelSettings,
                AmountModelSettings,
                predictionLevels
            ) {
                TransformType = TransformType,
                FrequencyModelSettings = FrequencyModelSettings,
                AmountModelSettings = AmountModelSettings,
                NumberOfMonteCarloIterations = NumberOfMonteCarloIterations,
                VarianceRatio = VarianceRatio,
                FixedDispersion = FixedDispersion
            };
        }

        private List<IndividualFrequency> GetFrequencyPredictionLevels(
            FrequencyDataResult dm,
            List<double> frequencies
        ) {
            var results = new List<IndividualFrequency>();
            for (int i = 0; i < dm.X.GetLength(0); i++) {
                var prediction = 0D;
                var j = 0;
                foreach (var freq in frequencies) {
                    prediction += dm.X[i, j] * freq;
                    j++;
                }
                results.Add(new IndividualFrequency(dm.SimulatedIndividuals?[i]) {
                    Prediction = UtilityFunctions.ILogit(prediction),
                    Cofactor = dm.Cofactor?[i],
                    Covariable = dm.Covariable?[i] ?? double.NaN,
                    NumberOfIndividuals = dm.GroupCounts[i],
                    Nbinomial = (int)(dm.Nbin?[i] ?? 0),
                    Frequency = dm.Ybin?[i] ?? double.NaN,
                });
            }
            return results;
        }

        /// <summary>
        /// Get prediction levels for grouped data.
        /// </summary>
        /// <param name="dm"></param>
        /// <returns></returns>
        private List<ModelledIndividualAmount> GetAmountsPredictionLevels(
            AmountDataResult dm,
            List<double> estimates
        ) {
            var results = new List<ModelledIndividualAmount>();
            for (int i = 0; i < dm.GroupCounts.Count; i++) {
                var prediction = estimates[0];
                for (int j = 1; j < estimates.Count; j++) {
                    prediction += dm.X[i, j - 1] * estimates[j];
                }
                results.Add(new ModelledIndividualAmount(dm.SimulatedIndividuals?[i]) {
                    Prediction = prediction,
                    Cofactor = dm.Cofactors?[i],
                    Covariable = dm.Covariables?[i] ?? double.NaN,
                    NumberOfIndividuals = dm.GroupCounts[i],
                });
            }
            return results;
        }

        public double GetConditionalFrequencyPrediction(
            CovariateGroup targetCovariateGroup,
            out CovariateGroup actualCovariateGroup
        ) {
            var frequencyPrediction = _conditionalFrequencyPredictions
                   .Where(f => {
                       double? covar = double.IsNaN(f.Covariable) ? null : f.Covariable;
                       double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : targetCovariateGroup.Covariable;
                       return (f.Cofactor == targetCovariateGroup.Cofactor && covar == targetCovar)
                           || (f.Cofactor == targetCovariateGroup.Cofactor && covar == null && targetCovar != null)
                           || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == targetCovar)
                           || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == null && targetCovar != null)
                          ;
                   })
                   .Select(f => (
                       CovariateGroup: new CovariateGroup {
                           Cofactor = f.Cofactor,
                           Covariable = f.Covariable,
                           NumberOfIndividuals = f.NumberOfIndividuals,
                       },
                       prediction: f.Prediction
                   ))
                  .Single();

            actualCovariateGroup = frequencyPrediction.CovariateGroup;

            return UtilityFunctions.Logit(frequencyPrediction.prediction);
        }

        public double GetConditionalAmountPrediction(
            CovariateGroup targetCovariateGroup,
            out CovariateGroup actualCovariateGroup
        ) {
            var amountPrediction = _conditionalAmountsPredictions
                .Where(f => {
                    double? covar = double.IsNaN(f.Covariable) ? null : f.Covariable;
                    double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : targetCovariateGroup.Covariable;
                    return (f.Cofactor == targetCovariateGroup.Cofactor && covar == targetCovar)
                        || (f.Cofactor == targetCovariateGroup.Cofactor && covar == null && targetCovar != null)
                        || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == targetCovar)
                        || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == null && targetCovar != null);
                })
                .Select(f => (
                    CovariateGroup: new CovariateGroup {
                        Cofactor = f.Cofactor,
                        Covariable = f.Covariable,
                        NumberOfIndividuals = f.NumberOfIndividuals,
                    },
                    Prediction: f.Prediction
                ))
                .Single();

            actualCovariateGroup = amountPrediction.CovariateGroup;
            return amountPrediction.Prediction;
        }

        public double GetSpecifiedFrequencyPrediction(
            CovariateGroup targetCovariateGroup,
            out CovariateGroup actualCovariateGroup
        ) {
            var frequencyPrediction = _specifiedFrequencyPredictions
                   .Where(f => {
                       double? covar = double.IsNaN(f.Covariable) ? null : f.Covariable;
                       double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : targetCovariateGroup.Covariable;
                       return (f.Cofactor == targetCovariateGroup.Cofactor && covar == targetCovar)
                           || (f.Cofactor == targetCovariateGroup.Cofactor && covar == null && targetCovar != null)
                           || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == targetCovar)
                           || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == null && targetCovar != null);
                   })
                   .Select(f => (
                       CovariateGroup: new CovariateGroup {
                           Cofactor = f.Cofactor,
                           Covariable = f.Covariable,
                           NumberOfIndividuals = f.NumberOfIndividuals,
                       },
                       prediction: f.Prediction
                   ))
                  .Single();
            actualCovariateGroup = frequencyPrediction.CovariateGroup;
            return UtilityFunctions.Logit(frequencyPrediction.prediction);
        }

        public double GetSpecifiedAmountPrediction(
            CovariateGroup targetCovariateGroup,
            out CovariateGroup actualCovariateGroup
        ) {
            var amountPrediction = _specifiedAmountsPredictions
                .Where(f => {
                    double? covar = double.IsNaN(f.Covariable) ? null : f.Covariable;
                    double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : targetCovariateGroup.Covariable;
                    return (f.Cofactor == targetCovariateGroup.Cofactor && covar == targetCovar)
                        || (f.Cofactor == targetCovariateGroup.Cofactor && covar == null && targetCovar != null)
                        || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == targetCovar)
                        || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == null && targetCovar != null);
                })
                .Select(f => (
                    CovariateGroup: new CovariateGroup {
                        Cofactor = f.Cofactor,
                        Covariable = f.Covariable,
                        NumberOfIndividuals = f.NumberOfIndividuals,
                    },
                    Prediction: f.Prediction
                ))
                .Single();

            actualCovariateGroup = amountPrediction.CovariateGroup;
            return amountPrediction.Prediction;
        }

        /// <summary>
        /// Build variance covariance matrix
        /// </summary>
        private static double[,] computeVcovMatrix(
            double dispersion,
            double varianceBetween,
            double correlation
        ) {
            var vcov = new double[2, 2];
            vcov[0, 0] = dispersion;
            vcov[1, 1] = varianceBetween;
            vcov[1, 0] = vcov[0, 1] = correlation * Math.Sqrt(vcov[0, 0] * vcov[1, 1]);
            return vcov;
        }

        /// <summary>
        /// Fit LNN model with correlation
        /// </summary>
        /// <param name="individualDayIntakes"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        private (LNNParameters estimates, LNNParameters stdErrors, double logLik, ErrorMessages msg) computeLNNModel(
            ICollection<SimpleIndividualDayIntake> individualDayIntakes,
            double power
        ) {
            individualDayIntakes = [.. individualDayIntakes.OrderBy(c => c.SimulatedIndividual.Id)];

            var dfPolynomialFrequency = 0;
            if (FrequencyInitials.LikelihoodRatioTestResults != null) {
                dfPolynomialFrequency = FrequencyInitials.LikelihoodRatioTestResults.SelectedOrder;
            }
            var frequencyDataResult = individualDayIntakes.GetDMFrequencyLNN(
                FrequencyModelSettings.CovariateModelType,
                dfPolynomialFrequency
            );

            var dfPolynomialAmount = 0;
            if (AmountInitials.LikelihoodRatioTestResults != null) {
                dfPolynomialAmount = AmountInitials.LikelihoodRatioTestResults.SelectedOrder;
            }

            var amountDataResult = individualDayIntakes.GetDMAmountLNN(
                AmountModelSettings.CovariateModelType,
                dfPolynomialAmount
            );

            var amounts = individualDayIntakes.Select(r => r.Amount).ToList();
            var weights = individualDayIntakes.Select(r => r.SimulatedIndividual.SamplingWeight).ToList();
            var individualIntakeFrequencies = SimpleIndividualDayIntakesCalculator
                .ComputeIndividualAmounts(individualDayIntakes)
                .Select(c => c.NumberOfDays)
                .ToList();

            var initialParameterEstimate = new LNNParameters(
                FrequencyInitials,
                AmountInitials,
                power
            );

            var calculator = new LNNModelCalculator();
            var result = calculator.ComputeFit(
                amounts,
                weights,
                individualIntakeFrequencies,
                frequencyDataResult.X,
                amountDataResult.X,
                AmountInitials.IntakeTransformer,
                initialParameterEstimate
            );

            return (result.Parameters, result.StandardErrors, -result.LogLik, result.ErrorMessages);
        }

        /// <summary>
        /// Predictions LNN model with correlation
        /// </summary>
        /// <param name="individualDayIntakes"></param>
        /// <param name="estimates"></param>
        private void designsLNNModel(
            ICollection<SimpleIndividualDayIntake> individualDayIntakes,
            LNNParameters estimates
        ) {
            //Frequencies
            var dfPolynomialFrequency = 0;
            if (FrequencyInitials.LikelihoodRatioTestResults != null) {
                dfPolynomialFrequency = FrequencyInitials.LikelihoodRatioTestResults.SelectedOrder;
            }

            var frequencyDataResult = individualDayIntakes.GetDMFrequencyLNN(
                FrequencyModelSettings.CovariateModelType,
                dfPolynomialFrequency
            );

            var conditionalFrequencyDataResult = individualDayIntakes.GetDMConditionalPredictionsLNN(
                FrequencyModelSettings.CovariateModelType,
                frequencyDataResult
            );

            _conditionalFrequencyPredictions = GetFrequencyPredictionLevels(
                conditionalFrequencyDataResult,
                estimates.FreqEstimates
            );

            _specifiedFrequencyPredictions = GetFrequencyPredictionLevels(
                individualDayIntakes.GetDMSpecifiedPredictionsLNN(
                    FrequencyModelSettings.CovariateModelType,
                    frequencyDataResult,
                    predictionLevels
                ),
                estimates.FreqEstimates
            );

            //Amounts
            var dfPolynomialAmount = 0;
            if (AmountInitials.LikelihoodRatioTestResults != null) {
                dfPolynomialAmount = AmountInitials.LikelihoodRatioTestResults.SelectedOrder;
            }

            var amountDataResult = individualDayIntakes.GetDMAmountLNN(
                AmountModelSettings.CovariateModelType,
                dfPolynomialAmount
            );

            var conditionalAmountDataResult = individualDayIntakes.GetDMConditionalPredictionsLNN(
                AmountModelSettings.CovariateModelType,
                amountDataResult
            );

            _conditionalAmountsPredictions = GetAmountsPredictionLevels(
                conditionalAmountDataResult,
                estimates.AmountEstimates
            );

            _specifiedAmountsPredictions = GetAmountsPredictionLevels(
                individualDayIntakes.GetDMSpecifiedPredictionsLNN(
                    AmountModelSettings.CovariateModelType,
                    amountDataResult,
                    predictionLevels
                ),
                estimates.AmountEstimates
            );
        }
    }
}
