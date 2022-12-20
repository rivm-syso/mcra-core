using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeTransformers;
using MCRA.Simulation.Calculators.IntakeModelling.Integrators;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public sealed class NormalAmountsModel : AmountsModelBase {

        private RemlResult _remlResult;
        private List<ModelledIndividualAmount> _individualPredictions;
        private ICollection<ModelledIndividualAmount> _individualIntakeAmounts;
        private ModelResult _modelResult;

        public IntakeTransformer IntakeTransformer { get; set; }

        public override AmountsModelSummary CalculateParameters(
            ICollection<SimpleIndividualIntake> individualAmounts,
            List<double> predictionLevels
        ) {
            IntakeTransformer = IntakeTransformerFactory.Create(
                TransformType,
                () => PowerTransformer.CalculatePower(individualAmounts.Select(idi => idi.Intake))
            );
            TransformType = IntakeTransformer.TransformType;

            var transformedPositiveIndividualAmounts = 
                ComputeTransformedPositiveIndividualAmounts(individualAmounts, IntakeTransformer);

            //Determine whether individuals have more than 1 day
            var isOneDay = individualAmounts.All(c => c.NumberOfPositiveIntakeDays <= 1);

            AmountDataResult amountDataResult = null;
            var modelResults = new List<ModelResult>();
            LikelihoodRatioTestResults LikelihoodRatioTest = null;
            if (CovariateModel != CovariateModelType.Constant && CovariateModel != CovariateModelType.Cofactor) {
                if (TestingMethod == TestingMethodType.Backward) {
                    for (int dfPol = MaxDegreesOfFreedom; dfPol > MinDegreesOfFreedom - 1; dfPol--) {
                        amountDataResult = transformedPositiveIndividualAmounts.GetDMAmount(CovariateModel, dfPol);
                        modelResults.Add(FitAmountsModel(amountDataResult, isOneDay));
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
                        amountDataResult = transformedPositiveIndividualAmounts.GetDMAmount(CovariateModel, dfPol);
                        modelResults.Add(FitAmountsModel(amountDataResult, isOneDay));
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
                        };
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

            amountDataResult = transformedPositiveIndividualAmounts.GetDMAmount(CovariateModel, dfPolynomial);
            _modelResult = FitAmountsModel(amountDataResult, isOneDay);

            var transformedIndividualAmounts = SimpleIndividualDayIntakesCalculator
                .ComputeMeanTransformedIndividualAmounts(individualAmounts, IntakeTransformer);

            _individualIntakeAmounts = transformedIndividualAmounts;

            if (IsAcuteCovariateModelling) {
                AcuteCovariateModelling(amountDataResult, transformedIndividualAmounts);
            }

            //Estimate specified conditional predictions for normal amounts model
            var specifiedPredictions = _individualIntakeAmounts.GetDMSpecifiedPredictions(
                CovariateModel,
                amountDataResult,
                predictionLevels
            );
            SpecifiedPredictions = GetPredictionLevels(specifiedPredictions);

            //Estimate conditional predictions for normal amounts model
            var conditionalPredictions = _individualIntakeAmounts.GetDMConditionalPredictions(CovariateModel, amountDataResult);
            ConditionalPredictions = GetPredictionLevels(conditionalPredictions);

            //Estimate individual predictions for normal amounts model
            var individualPredictions = _individualIntakeAmounts.GetDMIndividualPredictions(CovariateModel, amountDataResult);
            _individualPredictions = GetPredictionLevels(individualPredictions);

            var amountModelEstimates = new List<ParameterEstimates>();
            var parametername = "constant";
            for (int i = 0; i < _remlResult.Estimates.Count; i++) {
                if (i > 0) {
                    parametername = amountDataResult.DesignMatrixDescriptions[i - 1];
                }
                amountModelEstimates.Add(new ParameterEstimates() {
                    ParameterName = parametername,
                    Estimate = _remlResult.Estimates[i],
                    StandardError = _remlResult.Se[i],
                });
            }

            return new NormalAmountsModelSummary() {
                AmountModelEstimates = amountModelEstimates,
                VarianceBetween = _remlResult.VarianceBetween,
                VarianceWithin = _remlResult.VarianceWithin,
                VarianceDistribution = _remlResult.VarianceDistribution,
                _2LogLikelihood = _modelResult._2LogLikelihood,
                DegreesOfFreedom = _modelResult.DegreesOfFreedom,
                IntakeTransformer = IntakeTransformer,
                Residuals = _remlResult.Residuals,
                Blups = _remlResult.Blups,
                LikelihoodRatioTestResults = LikelihoodRatioTest,
            };
        }

        /// <summary>
        /// Change variance components for an acute exposure analysis with covariates.
        /// </summary>
        /// <param name="amountDataResult"></param>
        private void AcuteCovariateModelling(
            AmountDataResult amountDataResult,
            ICollection<ModelledIndividualAmount> transformedIndividualAmounts
        ) {
            var sumOfSquares = 0D;
            var individualFitRecords = _remlResult.FittedValues.Zip(amountDataResult.IndividualIds, (fv, id) => (fittedValue: fv, id: id)).ToList(); ;
            var distinctIdLevels = amountDataResult.IndividualIds.Distinct().ToList();

            var positiveIndividualDayIntakes = transformedIndividualAmounts
                .SelectMany(r => r.TransformedDayAmounts, (ia, a) => (
                    SimulatedIndividualId: ia.SimulatedIndividualId,
                    Amount: a
                ))
                .ToList();

            _remlResult.Residuals.Clear();
            _remlResult.FittedValues.Clear();
            foreach (var id in distinctIdLevels) {
                var fittedValue = individualFitRecords
                    .Where(c => c.id == id)
                    .Select(c => c.fittedValue)
                    .First();
                var residuals = positiveIndividualDayIntakes
                    .Where(r => r.SimulatedIndividualId == id)
                    .Select(r => r.Amount - fittedValue)
                    .ToList();
                _remlResult.Residuals.AddRange(residuals);
                _remlResult.FittedValues.AddRange(Enumerable.Repeat(fittedValue, residuals.Count));
                sumOfSquares += residuals.Select(y => Math.Pow(y, 2)).Sum();
            }
            _remlResult.VarianceDistribution = sumOfSquares / (amountDataResult.IndividualSamplingWeights.Sum() - 1);
            _remlResult.VarianceBetween = _remlResult.VarianceDistribution;
            _remlResult.VarianceWithin = 0D;
        }

        /// <summary>
        /// Fit amounts model
        /// </summary>
        /// <param name="adr"></param>
        /// <param name="isOneDay"></param>
        /// <returns></returns>
        private ModelResult FitAmountsModel(AmountDataResult adr, bool isOneDay) {
            if (isOneDay) {
                var X = DesignUtils.AddConstantColumn(adr.X, adr.Ys.Count);
                var mlrResults = MultipleLinearRegressionCalculator.Compute(X, adr.Ys, adr.IndividualSamplingWeights);
                var variancBetween = mlrResults.Sigma2 * VarianceRatio / (1 + VarianceRatio);
                var varianceWithin = mlrResults.Sigma2 - variancBetween;
                var factor = mlrResults.Sigma2 != 0 ? Math.Sqrt(variancBetween / (variancBetween + varianceWithin)) : 1;
                var blups = new List<double>();
                var fittedValues = new List<double>();
                var errors = new List<double>();
                for (int i = 0; i < adr.Ys.Count(); i++) {
                    var blup = mlrResults.Residuals[i] * factor;
                    blups.Add(blup);
                    fittedValues.Add(mlrResults.FittedValues[i] + blup);
                    errors.Add(mlrResults.Residuals[i] + blup);
                }
                _remlResult = new RemlResult() {
                    Df = mlrResults.DegreesOfFreedom,
                    _2LogLikelihood = mlrResults.MeanDeviance * mlrResults.DegreesOfFreedom,
                    Estimates = mlrResults.RegressionCoefficients,
                    Se = mlrResults.StandardErrors,
                    VarianceBetween = variancBetween,
                    VarianceWithin = varianceWithin,
                    FittedValues = fittedValues,
                    Residuals = errors,
                    Blups = blups,
                };
                return new ModelResult() {
                    DegreesOfFreedom = _remlResult.Df,
                    DfPolynomial = adr.DfPolynomial,
                    _2LogLikelihood = _remlResult._2LogLikelihood,
                    Estimates = _remlResult.Estimates,
                    StandardErrors = _remlResult.Se,
                };
            } else {
                //Normal amounts model (LME4, Mixed Model) is estimated
                _remlResult = MixedModelCalculator.FitMixedModel(adr.X, adr.Ys, adr.IndividualIds, adr.IndividualSamplingWeights);
                return new ModelResult() {
                    DegreesOfFreedom = _remlResult.Df,
                    DfPolynomial = adr.DfPolynomial,
                    _2LogLikelihood = _remlResult._2LogLikelihood,
                    Estimates = _remlResult.Estimates,
                    StandardErrors = _remlResult.Se,
                };
            }
        }

        /// <summary>
        /// Get prediction levels for groups on the linear scale for grouped data.
        /// </summary>
        /// <param name="dm"></param>
        /// <returns></returns>
        private List<ModelledIndividualAmount> GetPredictionLevels(AmountDataResult dm) {
            var results = new List<ModelledIndividualAmount>();
            for (int i = 0; i < dm.GroupCounts.Count; i++) {
                double prediction = _remlResult.Estimates[0];
                for (int j = 1; j < _remlResult.Estimates.Count; j++) {
                    prediction += dm.X[i, j - 1] * _remlResult.Estimates[j];
                }
                results.Add(new ModelledIndividualAmount() {
                    Prediction = prediction,
                    Cofactor = dm.Cofactors?[i],
                    Covariable = dm.Covariables?[i] ?? double.NaN,
                    NumberOfIndividuals = dm.GroupCounts[i],
                    NumberOfPositiveIntakeDays = dm.NDays?[i] ?? 0,
                    TransformedAmount = dm.Amounts?[i] ?? double.NaN,
                    SimulatedIndividualId = dm.IndividualIds?[i] ?? 0,
                    IndividualSamplingWeight = dm.IndividualSamplingWeights?[i] ?? double.NaN
                });
            }
            return results.OrderBy(c => c.SimulatedIndividualId).ToList();
        }

        protected override ICollection<ModelledIndividualAmount> CalculateModelAssistedAmounts(int seed) {

            ////convert BLUP to modified BLUP
            var random = new McraRandomGenerator(seed, true);

            var backTransform = IntakeTransformerFactory.Create(
                TransformType,
                () => (IntakeTransformer as PowerTransformer)?.Power ?? double.NaN
            );

            foreach (var item in _individualPredictions) {
                var factor = _remlResult.VarianceBetween / (_remlResult.VarianceBetween + _remlResult.VarianceWithin / item.NumberOfPositiveIntakeDays);
                item.ShrinkageFactor = Math.Sqrt(factor);

                // If the person has no consumption, then draw an amount according to the model
                if (double.IsNaN(item.TransformedAmount)) {
                    item.ModelAssistedAmount = Math.Sqrt(_remlResult.VarianceBetween) * NormalDistribution.InvCDF(0, 1, random.NextDouble()) + item.Prediction;
                } else {
                    item.ModelAssistedAmount = item.Prediction + (item.TransformedAmount - item.Prediction) * Math.Sqrt(factor);
                }
                item.BackTransformedAmount = backTransform.BiasCorrectedInverseTransform(item.ModelAssistedAmount, _remlResult.VarianceWithin);
            }
            return _individualPredictions;
        }

        protected override ConditionalPredictionResults GetConditionalPredictionsResults() {
            return new ConditionalPredictionResults() {
                ConditionalPrediction = ConditionalPredictions.Select(c => new ConditionalPrediction() {
                    Prediction = c.Prediction,
                    Cofactor = c.Cofactor,
                    Covariable = c.Covariable,
                })
                .ToList(),
                ConditionalData = _individualIntakeAmounts.Select(c => new ConditionalPrediction() {
                    Prediction = c.TransformedAmount,
                    Cofactor = c.Cofactor,
                    Covariable = c.Covariable,
                })
                .ToList(),
            };
        }

        /// <summary>
        /// Returns the individual amounts distribution for the specified covariate group.
        /// </summary>
        /// <param name="predictions"></param>
        /// <param name="targetCovariateGroup"></param>
        /// <param name="actualCovariateGroup"></param>
        /// <returns></returns>
        public override TransformBase GetDistribution(
            List<ModelledIndividualAmount> predictions,
            CovariateGroup targetCovariateGroup,
            out CovariateGroup actualCovariateGroup
        ) {
            if (!predictions.Any()) {
                actualCovariateGroup = null;
                return new TransformFallback() { };
            }

            var amountPrediction = predictions
                .Where(f => isMatchCovariateGroup(f, targetCovariateGroup))
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
            switch (TransformType) {
                case TransformType.NoTransform:
                    return new TransformIdentity() {
                        Mu = amountPrediction.Prediction,
                        VarianceBetween = _remlResult.VarianceBetween,
                        VarianceWithin = _remlResult.VarianceWithin
                    };
                case TransformType.Logarithmic:
                    return new TransformLogarithmic() {
                        Mu = amountPrediction.Prediction,
                        VarianceBetween = _remlResult.VarianceBetween,
                        VarianceWithin = _remlResult.VarianceWithin,
                    };
                case TransformType.Power:
                    return new TransformPower() {
                        Mu = amountPrediction.Prediction,
                        VarianceBetween = _remlResult.VarianceBetween,
                        VarianceWithin = _remlResult.VarianceWithin,
                        Power = ((PowerTransformer)IntakeTransformer).Power,
                        GaussHermitePoints = UtilityFunctions.GaussHermite(UtilityFunctions.GaussHermitePoints),
                    };
                default:
                    return null;
            }
        }
    }
}
