using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.UnitVariabilityCalculation {
    /// <summary>
    /// Calculates unit variability
    /// </summary>
    public sealed class UnitVariabilityCalculator {
        private readonly UnitVariabilityModelType _unitVariabilityModelType;
        private readonly UnitVariabilityType _unitVariabilityType;
        private readonly EstimatesNature _estimatesNature;
        private readonly int _defaultFactorLow;
        private readonly int _defaultFactorMid;
        private readonly MeanValueCorrectionType _meanValueCorrectionType;
        private readonly UnitVariabilityCorrelationType _unitVariabilityCorrelationType;

        private ConcurrentDictionary<Food, FoodUnitVariabilityInfo> _variabilityFactors;
        private ConcurrentDictionary<(Food, Compound, ProcessingType), UnitVariabilityModel> _unitVariabilityModels = new();

        public UnitVariabilityCalculator(
            UnitVariabilityModelType unitVariabilityModelType,
            UnitVariabilityType unitVariabilityType,
            EstimatesNature estimatesNature,
            int defaultFactorLow,
            int defaultFactorMid,
            MeanValueCorrectionType meanValueCorrectionType,
            UnitVariabilityCorrelationType unitVariabilityCorrelationType,
            Dictionary<Food, FoodUnitVariabilityInfo> variabilityFactors
        ) {
            _unitVariabilityModelType = unitVariabilityModelType;
            _unitVariabilityType = unitVariabilityType;
            _estimatesNature = estimatesNature;
            _defaultFactorLow = defaultFactorLow;
            _defaultFactorMid = defaultFactorMid;
            _meanValueCorrectionType = meanValueCorrectionType;
            _unitVariabilityCorrelationType = unitVariabilityCorrelationType;

            if (variabilityFactors != null) {
                _variabilityFactors = new ConcurrentDictionary<Food, FoodUnitVariabilityInfo>(variabilityFactors);
            } else {
                _variabilityFactors = new ConcurrentDictionary<Food, FoodUnitVariabilityInfo>();
            }
        }

        /// <summary>
        /// Splits the (consumed) amount in portions based the most specific variability factor associated
        /// with foodConsumed, and calculates a new concentration for each portion.
        /// </summary>
        /// <param name="foodAsMeasured">The target food-as-measured</param>
        /// <param name="amount">The amound consumed</param>
        /// <param name="processingType">The type of processing applied</param>
        /// <param name="compoundConcentrations">A collection of substance concentrations</param>
        /// <param name="correlationType">Specifies whether or not the stochastic variability factors for the different substances are correlated or not.</param>
        /// <returns></returns>
        /// <remarks>When maximum correlation is selected, a single random number is drawn from a uniform
        /// distribution, which is used to calculate the unitvariability for all substances. i.e. if the
        /// stochastic variability-factor is larger than 1 for substance A, it will also be larger than 1
        /// for Compounds B and C.
        /// When NoCorrelation is selected, a different random number is draw for each individual substance.
        /// </remarks>
        public List<DietaryIntakePerCompound> CalculateResidues(
                List<DietaryIntakePerCompound> compoundConcentrations,
                Food foodAsMeasured,
                IRandom random
            ) {
            var intakesPerCompound = new List<DietaryIntakePerCompound>(compoundConcentrations.Count);
            var uvRandom = new CapturingGenerator(random);
            uvRandom.StartCapturing();
            foreach (var c in compoundConcentrations) {
                if (c.ProcessingType != null && c.ProcessingType.IsBulkingBlending) {
                    intakesPerCompound.Add(new DietaryIntakePerCompound() {
                        ProcessingCorrectionFactor = c.ProcessingCorrectionFactor,
                        ProcessingFactor = c.ProcessingFactor,
                        ProcessingType = c.ProcessingType,
                        Compound = c.Compound,
                        IntakePortion = c.IntakePortion,
                        UnitIntakePortions = c.UnitIntakePortions,
                    });
                } else {
                    var unitVariabilityModel = GetOrCreateUnitVariabilityModel(foodAsMeasured, c.Compound, c.ProcessingType);
                    if (unitVariabilityModel != null) {
                        if (_unitVariabilityCorrelationType == UnitVariabilityCorrelationType.FullCorrelation) {
                            uvRandom.Repeat();
                        }
                        intakesPerCompound.Add(new DietaryIntakePerCompound() {
                            Compound = c.Compound,
                            ProcessingCorrectionFactor = c.ProcessingCorrectionFactor,
                            ProcessingFactor = c.ProcessingFactor,
                            ProcessingType = c.ProcessingType,
                            IntakePortion = c.IntakePortion,
                            UnitIntakePortions = unitVariabilityModel.DrawFromDistribution(foodAsMeasured, c.IntakePortion, uvRandom),
                        });
                    } else {
                        intakesPerCompound.Add(new DietaryIntakePerCompound() {
                            Compound = c.Compound,
                            ProcessingCorrectionFactor = c.ProcessingCorrectionFactor,
                            ProcessingFactor = c.ProcessingFactor,
                            ProcessingType = c.ProcessingType,
                            IntakePortion = c.IntakePortion,
                            UnitIntakePortions = c.UnitIntakePortions,
                        });
                    }
                }
            }
            return intakesPerCompound;
        }

        /// <summary>
        /// Obtains the most specific variabilityfactor for a Food/Compound combination and stores it in the
        /// dictionary, if it was not already there.
        /// </summary>
        /// <param name="foodAsMeasured">The target food</param>
        /// <param name="compound">The target compound</param>
        /// <param name="processingType">The type of processing applied to the food (can be null)</param>
        /// <returns></returns>
        private UnitVariabilityFactor GetOrCreateMostSpecificVariabilityFactor(Food foodAsMeasured, Compound compound, ProcessingType processingType) {
            if (_variabilityFactors.ContainsKey(foodAsMeasured)) {
                var vf = _variabilityFactors[foodAsMeasured].GetOrCeateMostSpecificVariabilityFactor(compound, processingType, _defaultFactorLow, _defaultFactorMid);
            } else {
                var unitVariabilityInfo = new FoodUnitVariabilityInfo(foodAsMeasured, new List<UnitVariabilityFactor>());
                _variabilityFactors.TryAdd(foodAsMeasured, unitVariabilityInfo);
            }
            return _variabilityFactors[foodAsMeasured].GetOrCeateMostSpecificVariabilityFactor(compound, processingType, _defaultFactorLow, _defaultFactorMid);
        }

        /// <summary>
        /// Gets a unit variability model for the given variability factor from the internal dictionary.
        /// Creates the model if it does not yet exist.
        /// </summary>
        /// <param name="vf"></param>
        /// <returns></returns>
        private UnitVariabilityModel GetOrCreateUnitVariabilityModel(Food foodAsMeasured, Compound compound, ProcessingType processingType) {
            var variabilityFactor = GetOrCreateMostSpecificVariabilityFactor(foodAsMeasured, compound, processingType);
            var specificCompound = variabilityFactor.Compound != null ? compound : null;
            var specificProcessingType = variabilityFactor.ProcessingType != null ? processingType : null;
            if (!_unitVariabilityModels.ContainsKey((foodAsMeasured, specificCompound, specificProcessingType))) {
                var unitVariabilityModel = createUnitVariabilityModel(foodAsMeasured, variabilityFactor);
                if (!unitVariabilityModel.CalculateParameters()) {
                    unitVariabilityModel = new NoUnitVariabilityModel(foodAsMeasured, variabilityFactor);
                }
                _unitVariabilityModels.AddOrUpdate((foodAsMeasured, specificCompound, specificProcessingType), unitVariabilityModel, (k, v) => v);
            }
            return _unitVariabilityModels[(foodAsMeasured, specificCompound, specificProcessingType)];
        }

        /// <summary>
        /// VariabilityModel factory. Creates a UnitVariabilityModel that corresponds to Settings.ModelType.
        /// </summary>
        /// <param name="vf"></param>
        /// <returns></returns>
        private UnitVariabilityModel createUnitVariabilityModel(Food foodAsMeasured, UnitVariabilityFactor unitVariabilityFactor) {
            UnitVariabilityModel model = null;
            switch (_unitVariabilityModelType) {
                case UnitVariabilityModelType.BetaDistribution:
                    model = new BetaDistributionModel(foodAsMeasured, unitVariabilityFactor, _estimatesNature, _unitVariabilityType);
                    break;
                case UnitVariabilityModelType.LogNormalDistribution:
                    model = new LogNormalDistributionModel(foodAsMeasured, unitVariabilityFactor, _meanValueCorrectionType, _estimatesNature, _unitVariabilityType);
                    break;
                case UnitVariabilityModelType.BernoulliDistribution:
                    model = new BernoulliDistributionModel(foodAsMeasured, unitVariabilityFactor, _estimatesNature);
                    break;
            }
            return model;
        }
    }
}
