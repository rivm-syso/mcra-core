using MCRA.Utils;
using MCRA.Utils.NumericalRecipes;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeTransformers;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Logistic normal normal model including correlation (known as NCI model) for chronic exposure assessment 
    /// </summary>
    public class LNNModel : IntakeModel, IUncorrelatedIntakeModel {

        public TransformType TransformType { get; set; }

        public IIntakeModelCalculationSettings AmountModelSettings { get; set; }

        public IIntakeModelCalculationSettings FrequencyModelSettings { get; set; }

        public int NumberOfMonteCarloIterations { get; set; }

        public FrequencyModelSummary FrequencyInitials { get; set; }
        public NormalAmountsModelSummary AmountInitials { get; set; }
        public FrequencyAmountModelSummary FrequencyAmountModelSummary { get; set; }

        public LNN0Model Lnn0Model { get; set; }
        public IntakeModelType FallBackModel { get; set; }

        private IntakeTransformer intakeTransformer { get; set; }
        private LNN0Model lnn0Model;

        public List<double> PredictionLevels { get; set; }

        private List<ModelledIndividualAmount> conditionalAmountsPredictions;
        private List<ModelledIndividualAmount> specifiedAmountsPredictions;
        private List<IndividualFrequency> conditionalFrequencyPredictions;
        private List<IndividualFrequency> specifiedFrequencyPredictions;

        private List<ParameterEstimates> frequencyModelEstimates;
        private List<ParameterEstimates> amountModelEstimates;
        private ParameterEstimates dispersionEstimates;
        private ParameterEstimates correlationEstimates;
        private LNNModelCalculator lnnModel;
        private double[,] chol;
        private double power;

        /// <summary>
        /// Creates a new <see cref="UncorrelatedIntakeModel{TFrequencyModel, TAmountsModel}"/> instance.
        /// </summary>
        /// <param name="frequencyModelSettings"></param>
        /// <param name="amountModelSettings"></param>
        /// <param name="predictionLevels"></param>
        public LNNModel(
            IIntakeModelCalculationSettings frequencyModelSettings,
            IIntakeModelCalculationSettings amountModelSettings,
            List<double> predictionLevels = null
        ) {
            FrequencyModelSettings = frequencyModelSettings;
            AmountModelSettings = amountModelSettings;
            PredictionLevels = predictionLevels;
        }

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
                PredictionLevels,
                FrequencyModelSettings.CovariateModelType,
                AmountModelSettings.CovariateModelType
            );

            DataBasedCovariateGroups = covariateGroupCalculator
                .ComputeDataBasedCovariateGroups(individualDayIntakes);

            SpecifiedPredictionCovariateGroups = covariateGroupCalculator
                .ComputeSpecifiedPredictionsCovariateGroups(individualDayIntakes);

            lnn0Model = GetLnn0Model();
            lnn0Model.CalculateParameters(individualDayIntakes);

            FrequencyInitials = lnn0Model.FrequencyModelSummary;
            AmountInitials = (NormalAmountsModelSummary)lnn0Model.AmountsModelSummary;

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
                
                fitLNNModel(
                    individualDayIntakes,
                    PredictionLevels
                );

                createVarianceCovarianceMatrix();

                FrequencyAmountModelSummary = new FrequencyAmountModelSummary() {
                    CorrelationEstimates = correlationEstimates,
                    VarianceBetween = lnnModel.Estimates.VarianceBetween,
                    VarianceWithin = lnnModel.Estimates.VarianceWithin,
                    DispersionEstimates = dispersionEstimates,
                    FrequencyModelEstimates = frequencyModelEstimates,
                    AmountModelEstimates = amountModelEstimates,
                    IntakeTransformer = intakeTransformer,
                    DegreesOfFreedom = 0,
                    _2LogLikelihood = lnnModel.LogLik,
                    Power = power,
                    ErrorMessage = lnnModel.ErrorMessage,
                };
            } else {
                // LNN with correlation cannot be fitted because exposure is not incidental.
                // Fit BNN or LNN without correlation instead
                Lnn0Model = lnn0Model;
                FallBackModel = IntakeModelType.LNN0;
            }
        }

        public override List<ConditionalUsualIntake> GetConditionalIntakes(
            int seed,
            CompositeProgressState progressState = null
        ) {
            if (FallBackModel == IntakeModelType.LNN0) {
                return lnn0Model.GetConditionalIntakes(seed, progressState)
                    .OrderBy(c => c.CovariatesCollection.OverallCofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(c => c.CovariatesCollection.OverallCovariable)
                    .ToList();
            }

            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var predictionCovariateGroups = SpecifiedPredictionCovariateGroups;
            var ghTransformer = IntakeTransformerFactory.Create(TransformType, () => lnnModel.Estimates.Power);
            var results = predictionCovariateGroups
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(covariateGroup => {
                    var random = Simulation.IsBackwardCompatibilityMode 
                        ? new McraRandomGenerator(covariateGroup.GetHashCode() + seed, true)
                        : new McraRandomGenerator(RandomUtils.CreateSeed(seed, covariateGroup.GetHashCode()));
                    var freqMean = GetSpecifiedFrequencyPrediction(covariateGroup, out CovariateGroup fcg);
                    var amountMean = GetSpecifiedAmountPrediction(covariateGroup, out CovariateGroup acg);
                    var usualIntakes = new List<double>();
                    var mean = new List<double> { freqMean, amountMean };
                    var randomSequence = MultiVariateNormalDistribution.Draw(mean, chol, NumberOfMonteCarloIterations, random);
                    for (int i = 0; i < randomSequence.GetLength(0); i++) {
                        var f = UtilityFunctions.ILogit(randomSequence[i, 0]);
                        var a = ghTransformer.BiasCorrectedInverseTransform(randomSequence[i, 1], lnnModel.Estimates.VarianceWithin);
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
                return results.OrderBy(c => c.CovariatesCollection.AmountCofactor, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(c => c.CovariatesCollection.AmountCovariable)
                        .ThenBy(c => c.CovariatesCollection.FrequencyCovariable)
                        .ToList();
            } else if (results.Select(c => c.CovariatesCollection.FrequencyCofactor).First() != string.Empty) {
                return results.OrderBy(c => c.CovariatesCollection.FrequencyCofactor, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(c => c.CovariatesCollection.FrequencyCovariable)
                        .ThenBy(c => c.CovariatesCollection.AmountCovariable)
                        .ToList();
            } else {
                return results.OrderBy(c => c.CovariatesCollection.FrequencyCovariable)
                         .ThenBy(c => c.CovariatesCollection.AmountCovariable)
                         .ToList();
            }
        }

        public override List<ModelBasedIntakeResult> GetMarginalIntakes(
            int seed,
            CompositeProgressState progressState = null
        ) {
            if (FallBackModel == IntakeModelType.LNN0) {
                return lnn0Model.GetMarginalIntakes(seed);
            }
            var covariateGroups = DataBasedCovariateGroups;
            var n = NumberOfMonteCarloIterations / covariateGroups.Sum(c => c.NumberOfIndividuals);
            var numberOfIterationsPerIndividual = n > 0 ? n : 1;
            var ghTransformer = IntakeTransformerFactory.Create(TransformType, () => lnnModel.Estimates.Power);

            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            return covariateGroups
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(covariateGroup => {
                    var random = Simulation.IsBackwardCompatibilityMode
                        ? new McraRandomGenerator(covariateGroup.GetHashCode() + seed, true)
                        : new McraRandomGenerator(RandomUtils.CreateSeed(seed, covariateGroup.GetHashCode()));
                    var freqMean = GetConditionalFrequencyPrediction(covariateGroup, out CovariateGroup fcg);
                    var amountMean = GetConditionalAmountPrediction(covariateGroup, out CovariateGroup acg);
                    var usualIntakes = new List<double>();
                    var mean = new List<double> { freqMean, amountMean };
                    var randomSequence = MultiVariateNormalDistribution.Draw(mean, chol, covariateGroup.NumberOfIndividuals * numberOfIterationsPerIndividual, random);
                    for (int i = 0; i < randomSequence.GetLength(0); i++) {
                        var f = UtilityFunctions.ILogit(randomSequence[i, 0]);
                        var a = ghTransformer.BiasCorrectedInverseTransform(randomSequence[i, 1], lnnModel.Estimates.VarianceWithin);
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
                return lnn0Model.GetIndividualIntakes(seed);
            }
            return null;
        }

        public LNN0Model GetLnn0Model() {
            return new LNN0Model(
                FrequencyModelSettings,
                AmountModelSettings,
                PredictionLevels
            ) {
                TransformType = TransformType,
                FrequencyModelSettings = FrequencyModelSettings,
                AmountModelSettings = AmountModelSettings,
                NumberOfMonteCarloIterations = NumberOfMonteCarloIterations
            };
        }

        private List<IndividualFrequency> GetFrequencyPredictionLevels(FrequencyDataResult dm) {
            var results = new List<IndividualFrequency>();
            for (int i = 0; i < dm.X.GetLength(0); i++) {
                var prediction = 0D;
                var j = 0;
                foreach (var item in frequencyModelEstimates) {
                    prediction += dm.X[i, j] * item.Estimate;
                    j++;
                }
                results.Add(new IndividualFrequency() {
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
        private List<ModelledIndividualAmount> GetAmountsPredictionLevels(AmountDataResult dm) {
            var results = new List<ModelledIndividualAmount>();
            for (int i = 0; i < dm.GroupCounts.Count; i++) {
                var estimates = amountModelEstimates.Select(c => c.Estimate).ToList();
                var prediction = estimates[0];
                for (int j = 1; j < estimates.Count; j++) {
                    prediction += dm.X[i, j - 1] * estimates[j];
                }
                results.Add(new ModelledIndividualAmount() {
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
            var frequencyPrediction = conditionalFrequencyPredictions
                   .Where(f => {
                       double? covar = double.IsNaN(f.Covariable) ? null : (double?)f.Covariable;
                       double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : (double?)targetCovariateGroup.Covariable;
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
            var amountPrediction = conditionalAmountsPredictions
                .Where(f => {
                    double? covar = double.IsNaN(f.Covariable) ? null : (double?)f.Covariable;
                    double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : (double?)targetCovariateGroup.Covariable;
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
            var frequencyPrediction = specifiedFrequencyPredictions
                   .Where(f => {
                       double? covar = double.IsNaN(f.Covariable) ? null : (double?)f.Covariable;
                       double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : (double?)targetCovariateGroup.Covariable;
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
            var amountPrediction = specifiedAmountsPredictions
                .Where(f => {
                    double? covar = double.IsNaN(f.Covariable) ? null : (double?)f.Covariable;
                    double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : (double?)targetCovariateGroup.Covariable;
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
        private void createVarianceCovarianceMatrix() {
            var vcov = new double[2, 2];
            vcov[0, 0] = lnnModel.Estimates.Dispersion;
            vcov[1, 1] = lnnModel.Estimates.VarianceBetween;
            vcov[1, 0] = vcov[0, 1] = lnnModel.Estimates.Correlation * Math.Sqrt(vcov[0, 0] * vcov[1, 1]);
            chol = MatrixNR.Cholesky(vcov);
        }

        /// <summary>
        /// Fit LNN model
        /// </summary>
        /// <param name="individualDayIntakes"></param>
        /// <param name="predictionLevels"></param>
        private void fitLNNModel(
            ICollection<SimpleIndividualDayIntake> individualDayIntakes,
            List<double> predictionLevels
        ) {
            individualDayIntakes = individualDayIntakes
                .OrderBy(c => c.Individual.Id)
                .ToList();

            var imStatsOneDay = false;

            var lnnParameters = new LNNParameters() {
                EstimatePower = false,
                EstimateFrequency = true,
                EstimateDispersion = true,
                EstimateAmount = true,
                EstimateVarianceBetween = true,
                EstimateVarianceWithin = true,
                EstimateCorrelation = true,
                Power = power,
                Dispersion = FrequencyInitials.DispersionEstimates.Estimate,
                VarianceBetween = AmountInitials.VarianceBetween,
                VarianceWithin = AmountInitials.VarianceWithin,
                Correlation = 0D,
                FreqEstimates = FrequencyInitials.FrequencyModelEstimates.Select(c => c.Estimate).ToList(),
                AmountEstimates = AmountInitials.AmountModelEstimates.Select(c => c.Estimate).ToList(),
                TransformType = TransformType,
            };

            lnnParameters.Transform();

            // Set fixed dispersion parameters in case only one Day
            if (imStatsOneDay) {
                lnnParameters = new LNNParameters() {
                    EstimateDispersion = false,
                    EstimateVarianceBetween = false,
                    EstimateVarianceWithin = false,
                };
            }

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

            //Hier gaat toch echt iets niet goed, alles moet uit frequencyDataResult en amountDataResult komen en niet alleen een gedeelte (bv dailyIntakes weer niet)
            var conditionalFrequencyDataResult = individualDayIntakes.GetDMConditionalPredictionsLNN(
                FrequencyModelSettings.CovariateModelType,
                frequencyDataResult
            );

            var conditionalAmountDataResult = individualDayIntakes.GetDMConditionalPredictionsLNN(
                AmountModelSettings.CovariateModelType,
                amountDataResult
            );

            var individualIntakeFrequencies = SimpleIndividualDayIntakesCalculator
                .ComputeIndividualAmounts(individualDayIntakes);

            lnnModel = new LNNModelCalculator() {
                DailyIntakes = individualDayIntakes.Select(r => r.Amount).ToList(),
                Weights = individualDayIntakes.Select(r => r.IndividualSamplingWeight).ToList(),
                DailyIntakesTransformed = individualDayIntakes
                    .Select(r => AmountInitials.IntakeTransformer.Transform(r.Amount))
                    .ToList(),
                FreqDesign = frequencyDataResult.X,
                AmountDesign = amountDataResult.X,
                LNNParameters = lnnParameters,
                IndividualDays = individualIntakeFrequencies.Select(c => c.NumberOfDays).ToList(),
                FreqLp = new double[individualDayIntakes.Count()],
                AmountLp = new double[individualDayIntakes.Count()],
                GaussHermitePoints = 10,
                GaussHermitePrune = -1.0,
                MaxEvaluations = 200,
                Tolerance = 1.0 - 6,
                SeMaxCycle = 2,
                SeReturn = "none",
                ScaleAmountInAlgorithm = false,
                FreqPredictX = conditionalFrequencyDataResult.X,
                AmountPredictX = conditionalAmountDataResult.X,
            };

            lnnModel.Initialize();
            lnnModel.Fit();

            // Set parameters of intakeFrequency to NCI parameters
            frequencyModelEstimates = new List<ParameterEstimates>();
            var parameterNames = FrequencyInitials.FrequencyModelEstimates.Select(c => c.ParameterName).ToList();
            for (int i = 0; i < lnnModel.Estimates.FreqEstimates.Count; i++) {
                frequencyModelEstimates.Add(new ParameterEstimates() {
                    ParameterName = parameterNames[i],
                    Estimate = lnnModel.Estimates.FreqEstimates[i],
                    //StandardError = lnnModel.Se.FreqEstimates[i]
                });
            }
            dispersionEstimates = new ParameterEstimates() {
                ParameterName = "Dispersion",
                Estimate = lnnModel.Estimates.Dispersion,
                //StandardError = lnnModel.Se.Dispersion,
            };

            amountModelEstimates = new List<ParameterEstimates>();
            parameterNames = AmountInitials.AmountModelEstimates.Select(c => c.ParameterName).ToList();
            for (int i = 0; i < lnnModel.Estimates.AmountEstimates.Count; i++) {
                amountModelEstimates.Add(new ParameterEstimates() {
                    ParameterName = parameterNames[i],
                    Estimate = lnnModel.Estimates.AmountEstimates[i],
                    //StandardError = lnnModel.Se.AmountEstimates[i]
                });
            }

            correlationEstimates = new ParameterEstimates() {
                ParameterName = "Correlation",
                Estimate = lnnModel.Estimates.Correlation,
                //StandardError = lnnModel.Se.Correlation,
            };

            conditionalFrequencyPredictions = GetFrequencyPredictionLevels(conditionalFrequencyDataResult);
            conditionalAmountsPredictions = GetAmountsPredictionLevels(conditionalAmountDataResult);

            var specifiedFrequencyDataResult = individualDayIntakes.GetDMSpecifiedPredictionsLNN(
                FrequencyModelSettings.CovariateModelType,
                frequencyDataResult,
                predictionLevels
            );

            var specifiedAmountDataResult = individualDayIntakes.GetDMSpecifiedPredictionsLNN(
                AmountModelSettings.CovariateModelType,
                amountDataResult,
                predictionLevels
            );

            specifiedFrequencyPredictions = GetFrequencyPredictionLevels(specifiedFrequencyDataResult);
            specifiedAmountsPredictions = GetAmountsPredictionLevels(specifiedAmountDataResult);
        }
    }
}
