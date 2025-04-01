using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Wrappers {
    public sealed class FoodUnitVariabilityInfo {

        private object _lock = new();

        private UnitVariabilityFactor _defaultUnitVariabilityFactor;

        /// <summary>
        /// The food to which the unit variability info applies.
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// The unit variability factors that apply for the food of this unit variability info object.
        /// </summary>
        public List<UnitVariabilityFactor> UnitVariabilityFactors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FoodUnitVariabilityInfo{T}"/> class.
        /// </summary>
        /// <param name="food">The food to which the unit variability info applies.</param>
        /// <param name="unitVariabilityFactors">The unit variability factors that apply for the food of this unit variability info object.</param>
        public FoodUnitVariabilityInfo(Food food, IEnumerable<UnitVariabilityFactor> unitVariabilityFactors) {
            Food = food;
            UnitVariabilityFactors = unitVariabilityFactors.ToList();
        }

        /// <summary>
        /// This function creates a variabilityFactor for this food, based on it's FoodProperties. The Resulting
        /// variability factor is not associated with a substance.
        /// If it already has an associated VariabilityFactor, the original VariabilityFactor is returned.
        /// </summary>
        public UnitVariabilityFactor CreateDefaultVariabilityFactor(double defaultFactorLow, double defaultFactorMid) {
            lock (_lock) {
                if (Food.Properties == null || Food.Properties.UnitWeight == null) {
                    return new UnitVariabilityFactor() {
                        Factor = 1,
                    };
                }
                if (!UnitVariabilityFactors.Any(vf => vf.Compound == null && vf.ProcessingType == null)) {
                    if (_defaultUnitVariabilityFactor == null) {
                        var unitWeightRac = Food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac)?.Value ?? double.NaN;
                        _defaultUnitVariabilityFactor = new UnitVariabilityFactor(defaultFactorLow, defaultFactorMid, unitWeightRac) {
                            Food = Food,
                        };
                    }
                    return _defaultUnitVariabilityFactor;
                } else {
                    return UnitVariabilityFactors.First(vf => vf.Compound == null && vf.ProcessingType == null);
                }
            }
        }

        /// <summary>
        /// Returs the most specific variability factor for the target compound/processingtype combination.
        /// Compound and processingtype can be null.
        /// </summary>
        /// <param name="compound">The target compound</param>
        /// <param name="processingType">The type of processing applied</param>
        /// <returns></returns>
        public UnitVariabilityFactor GetMostSpecificVariabilityFactor(Compound compound, ProcessingType processingType) {
            UnitVariabilityFactor variabilityFactor = null;
            if (processingType != null && compound != null) {
                variabilityFactor = UnitVariabilityFactors.FirstOrDefault(vf => vf.Compound == compound && vf.ProcessingType == processingType);
            }
            if (variabilityFactor == null && compound != null) {
                variabilityFactor = UnitVariabilityFactors.FirstOrDefault(vf => vf.Compound == compound && vf.ProcessingType == null);
            }
            if (variabilityFactor == null) {
                variabilityFactor = UnitVariabilityFactors.FirstOrDefault(vf => vf.Compound == null && vf.ProcessingType == null);
            }
            return variabilityFactor;
        }

        /// <summary>
        /// Gets the most specific variability factor for the target compound/processingtype combination or
        /// create a new default processing factor if no matches were found.
        /// Compound and processingtype can be null.
        /// </summary>
        /// <param name="compound"></param>
        /// <param name="processingType"></param>
        /// <param name="defaultFactorLow"></param>
        /// <param name="defaultFactorMid"></param>
        /// <returns></returns>
        public UnitVariabilityFactor GetOrCeateMostSpecificVariabilityFactor(Compound compound, ProcessingType processingType, double defaultFactorLow, double defaultFactorMid) {
            UnitVariabilityFactor variabilityFactor = GetMostSpecificVariabilityFactor(compound, processingType);
            if (variabilityFactor == null) {
                variabilityFactor = CreateDefaultVariabilityFactor(defaultFactorLow, defaultFactorMid);
            }
            return variabilityFactor;
        }

        public bool HasVariabilityFactorAndVariationCoefficientData() {
            return HasVariabilityFactorData() && HasVariationCoefficientData();
        }

        public bool HasVariabilityFactorData() {
            return UnitVariabilityFactors.Any(vf => vf.Factor != null);
        }

        public bool HasVariationCoefficientData() {
            return UnitVariabilityFactors.Any(vf => vf.Coefficient != null);
        }

        public UnitVariabilityRecord GetUnitVariabilityRecord(Compound compound, ProcessingType processingType) {
            var uv = GetMostSpecificVariabilityFactor(compound, processingType);
            if (uv != null) {
                var uvRecord = new UnitVariabilityRecord {
                    UnitWeight = Food.GetDefaultUnitWeight(UnitWeightValueType.UnitWeightRac)?.Value ?? double.NaN,
                    UnitVariabilityFactor = uv.Factor ?? double.NaN,
                    UnitsInCompositeSample = (int)uv.UnitsInCompositeSample,
                    CoefficientOfVariation = uv.Coefficient ?? double.NaN,
                };
                return uvRecord;
            }
            return null;
        }
    }
}
