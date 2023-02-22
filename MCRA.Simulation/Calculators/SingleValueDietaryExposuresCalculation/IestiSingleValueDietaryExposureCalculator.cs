using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Compiled.Wrappers.UnitVariability;
using MCRA.General;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public class IestiSingleValueDietaryExposureCalculator : ISingleValueDietaryExposureCalculator {

        private readonly bool _isMrlSetting;
        private readonly bool _isApplyProcessingFactors;

        private static IESTIType[] _unitVariabilityCases = {
            IESTIType.Case2a,
            IESTIType.Case2b,
            IESTIType.CaseNew2a_2b
        };

        public SingleValueDietaryExposuresCalculationMethod CalculationMethod => _isMrlSetting
            ? SingleValueDietaryExposuresCalculationMethod.IESTINew
            : SingleValueDietaryExposuresCalculationMethod.IESTI;

        private readonly ProcessingFactorModelCollection _processingFactors;

        private readonly Dictionary<Food, FoodUnitVariabilityInfo> _unitVariabilityFactors;
        private readonly ICollection<IestiSpecialCase> _iestiSpecialCases;

        public IestiSingleValueDietaryExposureCalculator(
            ProcessingFactorModelCollection processingFactors,
            Dictionary<Food, FoodUnitVariabilityInfo> unitVariabilityFactors,
            ICollection<IestiSpecialCase> iestiSpecialCases,
            bool isMrlSetting
        ) {
            _isApplyProcessingFactors = processingFactors != null;
            _processingFactors = processingFactors;
            _unitVariabilityFactors = unitVariabilityFactors;
            _isMrlSetting = isMrlSetting;
            _iestiSpecialCases = iestiSpecialCases;
        }

        /// <summary>
        /// Computes IESTI single value dietary exposures.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="substances"></param>
        /// <param name="singleValueConsumptionModels"></param>
        /// <param name="singleValueConcentrationModels"></param>
        /// <param name="occurrenceFractions"></param>
        /// <param name="consumptionIntakeUnit"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="consumptionBodyWeightUnit"></param>
        /// <param name="targetUnit"></param>
        /// <returns></returns>
        public ICollection<ISingleValueDietaryExposure> Compute(
            Population population,
            ICollection<Compound> substances,
            ICollection<SingleValueConsumptionModel> singleValueConsumptionModels,
            IDictionary<(Food, Compound), SingleValueConcentrationModel> singleValueConcentrationModels,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrenceFractions,
            ConsumptionIntakeUnit consumptionIntakeUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit consumptionBodyWeightUnit,
            TargetUnit targetUnit
        ) {
            if (!targetUnit.IsPerBodyWeight()) {
                throw new NotImplementedException("IESTI calculation is not implemented for per-person.");
            }
            if (!consumptionIntakeUnit.IsPerPerson()) {
                throw new NotImplementedException("IESTI calculation is not implemented for per bodyweight consumptions.");
            }
            if (consumptionIntakeUnit.GetConsumptionUnit() != ConsumptionUnit.g) {
                throw new NotImplementedException("IESTI calculation is only implemented for consumptions in grams.");
            }
            var results = new List<ISingleValueDietaryExposure>();

            /// IESTI consumption unit is in grams
            var consumptionUnit = ConsumptionUnit.g;

            // Unit conversion factor for concentrations
            var conversionFactorConcentrationMassUnit = concentrationUnit.GetConcentrationMassUnit().GetMultiplicationFactor(ConcentrationMassUnitConverter.FromConsumptionUnit(consumptionUnit));
            var conversionFactorConcentrationAmountUnit = concentrationUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.SubstanceAmount, double.NaN);
            var concentrationUnitConversionFactor = conversionFactorConcentrationAmountUnit / conversionFactorConcentrationMassUnit;

            // Loop over the available single value consumptions
            foreach (var singleValueConsumptionModel in singleValueConsumptionModels) {
                var nominalBodyWeight = double.NaN;
                if (!double.IsNaN(singleValueConsumptionModel.BodyWeight)) {
                    var bodyWeightUnitConversionFactor = ConcentrationMassUnitConverter
                        .FromBodyWeightUnit(consumptionBodyWeightUnit)
                        .GetMultiplicationFactor(targetUnit.ConcentrationMassUnit);
                    nominalBodyWeight = bodyWeightUnitConversionFactor * singleValueConsumptionModel.BodyWeight;
                } else if (population != null && !double.IsNaN(population.NominalBodyWeight)) {
                    var bodyWeightUnitConversionFactor = ConcentrationMassUnitConverter
                        .FromBodyWeightUnit(population.BodyWeightUnit)
                        .GetMultiplicationFactor(targetUnit.ConcentrationMassUnit);
                    nominalBodyWeight = bodyWeightUnitConversionFactor * (double)population.NominalBodyWeight;
                } else {
                    throw new Exception($"No body weight specified for food consumption of {singleValueConsumptionModel.Food.Name} and no population bodyweight to fall back on.");
                }

                var food = singleValueConsumptionModel.Food;
                var baseFood = food.BaseFood ?? food;
                var isProcessedFood = singleValueConsumptionModel.ProcessingTypes?.Any() ?? false;

                if (baseFood.UnitWeightUnit != consumptionUnit) {
                    throw new NotImplementedException("IESTI calculation expects unit weights specified in grams.");
                }

                // Loop over the substances
                foreach (var substance in substances) {
                    // Consumption
                    var largePortionUnitConversionFactor = consumptionIntakeUnit.GetConsumptionUnit().GetMultiplicationFactor(consumptionUnit);
                    var largePortion = largePortionUnitConversionFactor * singleValueConsumptionModel.LargePortion;
                    var location = population?.Location ?? string.Empty;
                    var unitWeightEp = baseFood.GetUnitWeight(UnitWeightValueType.UnitWeightEp, location);
                    var unitWeightRac = baseFood.GetUnitWeight(UnitWeightValueType.UnitWeightRac, location);

                    // Processing type / processing factor
                    var processingTypes = singleValueConsumptionModel.ProcessingTypes;
                    var processingType = isProcessedFood ? processingTypes?.FirstOrDefault() : null;
                    var isBulkingBlending = processingType?.IsBulkingBlending ?? false;
                    var processingFactor = (processingTypes?.Any() ?? false)
                        ? _processingFactors?.GetNominalProcessingFactor(baseFood, substance, processingTypes.Last()) ?? double.NaN
                        : double.NaN;

                    // Occurrence frequency
                    var occurrenceFraction = getOccurenceFraction(baseFood, substance, occurrenceFractions);
                    var iestiSpecialCase = _iestiSpecialCases?.FirstOrDefault(r => r.Food == baseFood && r.Substance == substance);

                    // IESTI Case
                    var iestiCase = getIESTICase(
                        largePortion,
                        unitWeightRac,
                        unitWeightEp,
                        isBulkingBlending
                    );
                    iestiCase = getIESTISpecialCase(iestiCase, iestiSpecialCase);

                    // Unit variability factor
                    var isUnitVariability = _unitVariabilityCases.Contains(iestiCase);
                    var unitVariabilityFactor = isUnitVariability && (_unitVariabilityFactors?.ContainsKey(baseFood) ?? false)
                        ? (double)_unitVariabilityFactors[baseFood].GetMostSpecificVariabilityFactor(substance, null).Factor
                        : double.NaN;

                    // Concentration
                    singleValueConcentrationModels.TryGetValue((baseFood, substance), out var singleValueConcentrationModel);
                    (var concentration, var concentrationValueType) = getConcentrationValue(singleValueConcentrationModel, iestiCase);
                    concentration *= concentrationUnitConversionFactor;
                    if (double.IsNaN(concentration)) {
                        // If no concentration available, skip
                        continue;
                    }

                    // Exposure
                    var pf = _isApplyProcessingFactors && !double.IsNaN(processingFactor) ? processingFactor : 1D;
                    var exposurePerPerson = computeExposureAmount(
                        pf,
                        occurrenceFraction,
                        unitVariabilityFactor,
                        largePortion,
                        unitWeightEp,
                        iestiCase,
                        concentration
                    );
                    var result = new AcuteSingleValueDietaryExposureResult() {
                        Food = food,
                        Substance = substance,
                        IESTICase = iestiCase,
                        CalculationMethod = CalculationMethod,
                        LargePortion = largePortion,
                        ProcessingType = processingType,
                        ProcessingFactor = processingFactor,
                        MissingProcessingFactor = isProcessedFood && _isApplyProcessingFactors && double.IsNaN(processingFactor),
                        UnitVariabilityFactor = unitVariabilityFactor,
                        UnitWeightEp = baseFood.GetUnitWeight(UnitWeightValueType.UnitWeightEp, population?.Location),
                        UnitWeightRac = baseFood.GetUnitWeight(UnitWeightValueType.UnitWeightRac, population?.Location),
                        ConcentrationValueType = concentrationValueType,
                        ConcentrationValue = concentration,
                        Exposure = exposurePerPerson / nominalBodyWeight,
                        OccurrenceFraction = occurrenceFraction?.OccurrenceFrequency ?? 1,
                        BodyWeight = nominalBodyWeight,
                    };
                    results.Add(result);
                }
            }
            return results;
        }

        /// <summary>
        /// Computes the IESTI exposure amount part of the IESTI equations (i.e., not divided by BW).
        /// </summary>
        /// <param name="processingFactor"></param>
        /// <param name="occurrenceFraction"></param>
        /// <param name="variabilityFactor"></param>
        /// <param name="largePortion"></param>
        /// <param name="unitWeightEp"></param>
        /// <param name="iestiCase"></param>
        /// <param name="concentration"></param>
        /// <returns></returns>
        private double computeExposureAmount(
            double processingFactor,
            OccurrenceFraction occurrenceFraction,
            double variabilityFactor,
            double largePortion,
            QualifiedValue unitWeightEp,
            IESTIType iestiCase,
            double concentration
        ) {
            var useFrequency = occurrenceFraction?.OccurrenceFrequency ?? 1;
            switch (iestiCase) {
                case IESTIType.Case1:
                case IESTIType.Case3:
                case IESTIType.CaseNew1_3:
                    return largePortion * concentration * processingFactor * useFrequency;
                case IESTIType.Case2a:
                    if (double.IsNaN(variabilityFactor) || variabilityFactor == 0) {
                        return double.NaN;
                    }
                    var unitWeightEdiblePortion = unitWeightEp.Qualifier == ValueQualifier.Equals
                        ? unitWeightEp.Value
                        : double.NaN;
                    return unitWeightEdiblePortion * concentration * processingFactor * variabilityFactor * useFrequency
                        + (largePortion - unitWeightEdiblePortion) * concentration * processingFactor * useFrequency;
                case IESTIType.Case2b:
                    if (double.IsNaN(variabilityFactor) || variabilityFactor == 0) {
                        return double.NaN;
                    }
                    return largePortion * concentration * processingFactor * variabilityFactor * useFrequency;
                case IESTIType.CaseNew2a_2b:
                    if (double.IsNaN(variabilityFactor) || variabilityFactor == 0) {
                        return double.NaN;
                    }
                    return largePortion * concentration * processingFactor * variabilityFactor * useFrequency;
                case IESTIType.Undefined:
                    return double.NaN;
                default:
                    return double.NaN;
            }
        }

        /// <summary>
        /// Returns IESTI case based on unit weight edible portion and RAC and isBulkingBlending. New IESTI is NOT implemented
        /// </summary>
        /// <param name="largePortion"></param>
        /// <param name="unitWeightRac"></param>
        /// <param name="unitWeightEp"></param>
        /// <param name="isBulkingBlending"></param>
        /// <returns></returns>
        private IESTIType getIESTICase(
            double largePortion,
            QualifiedValue unitWeightRac,
            QualifiedValue unitWeightEp,
            bool isBulkingBlending) {
            if (CalculationMethod == SingleValueDietaryExposuresCalculationMethod.IESTI) {
                if (isBulkingBlending) {
                    return IESTIType.Case3;
                } else if (unitWeightRac?.IsNan() ?? true) {
                    return IESTIType.Undefined;
                } else if (unitWeightRac < new QualifiedValue(25)) {
                    return IESTIType.Case1;
                } else {
                    if (unitWeightEp?.IsNan() ?? true) {
                        return IESTIType.Undefined;
                    } else if (new QualifiedValue(largePortion) > unitWeightEp) {
                        return IESTIType.Case2a;
                    } else if (new QualifiedValue(largePortion) < unitWeightEp
                        || new QualifiedValue(largePortion) == unitWeightEp
                    ) {
                        return IESTIType.Case2b;
                    }
                }
            } else {
                if (isBulkingBlending) {
                    return IESTIType.CaseNew1_3;
                } else if (unitWeightRac?.IsNan() ?? true) {
                    return IESTIType.Undefined;
                } else if (unitWeightRac < new QualifiedValue(25)) {
                    return IESTIType.CaseNew1_3;
                } else {
                    return IESTIType.CaseNew2a_2b;
                }
            }
            return IESTIType.Undefined;
        }

        /// <summary>
        /// Returns IESTI case based post- or pre harvest type.
        /// </summary>
        /// <param name="iestiType"></param>
        /// <param name="iestiSpecialCase"></param>
        /// <returns></returns>
        private IESTIType getIESTISpecialCase(IESTIType iestiType, IestiSpecialCase iestiSpecialCase) {
            if (CalculationMethod == SingleValueDietaryExposuresCalculationMethod.IESTI) {
                if (iestiSpecialCase == null) {
                    return iestiType;
                } else if (iestiSpecialCase.ApplicationType == HarvestApplicationType.PostHarvest) {
                    return IESTIType.Case1;
                } else if (iestiSpecialCase.ApplicationType == HarvestApplicationType.PreHarvest) {
                    return IESTIType.Case3;
                }
            } else {
                if (iestiSpecialCase == null) {
                    return iestiType;
                } else {
                    return IESTIType.CaseNew1_3;
                }
            }
            return iestiType;
        }


        /// <summary>
        /// Get concentration based om MRL setting
        /// </summary>
        /// <param name="singleValueConcentrationModel"></param>
        /// <param name="iestiCase"></param>
        /// <returns></returns>
        protected (double, ConcentrationValueType) getConcentrationValue(
           SingleValueConcentrationModel singleValueConcentrationModel,
           IESTIType iestiCase
        ) {
            if (_isMrlSetting) {
                return (singleValueConcentrationModel?.Mrl ?? double.NaN, ConcentrationValueType.MRL);
            } else {
                var concentrationType = iestiCase == IESTIType.Case3
                    ? ConcentrationValueType.MedianConcentration
                    : ConcentrationValueType.HighestConcentration;
                if (singleValueConcentrationModel == null) {
                    return (double.NaN, concentrationType);
                } else if (concentrationType == ConcentrationValueType.HighestConcentration
                    && !double.IsNaN(singleValueConcentrationModel.HighestConcentration)
                ) {
                    return (singleValueConcentrationModel.HighestConcentration, concentrationType);
                } else if (concentrationType == ConcentrationValueType.MedianConcentration
                    && singleValueConcentrationModel.TryGetPercentile(50, out var percentile)
                ) {
                    return (percentile, concentrationType);
                } else {
                    return (singleValueConcentrationModel.Loq, ConcentrationValueType.LOQ);
                }
            }
        }

        private OccurrenceFraction getOccurenceFraction(
            Food food,
            Compound substance,
            IDictionary<(Food, Compound), OccurrenceFraction> occurrenceFractions
        ) {
            OccurrenceFraction occurrenceFraction;
            if (occurrenceFractions != null) {
                occurrenceFractions.TryGetValue((food, substance), out occurrenceFraction);
            } else {
                occurrenceFraction = null;
            }
            return occurrenceFraction;
        }
    }
}
