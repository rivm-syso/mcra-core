using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public class ChronicSingleValueDietaryExposureCalculator : ISingleValueDietaryExposureCalculator {

        private readonly bool _isMrlSetting;
        private readonly bool _isApplyProcessingFactors;

        private readonly ProcessingFactorModelCollection _processingFactors;

        public virtual SingleValueDietaryExposuresCalculationMethod CalculationMethod => _isMrlSetting
            ? SingleValueDietaryExposuresCalculationMethod.TMDI
            : SingleValueDietaryExposuresCalculationMethod.IEDI;

        public ChronicSingleValueDietaryExposureCalculator(
            ProcessingFactorModelCollection processingFactors,
            bool isMrlSetting
        ) {
            _processingFactors = processingFactors;
            _isApplyProcessingFactors = processingFactors != null;
            _isMrlSetting = isMrlSetting;
        }

        /// <summary>
        /// Calculates chronic single value dietary exposures, IEDI and TMDI
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
        ///
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
                throw new NotImplementedException("IEDI and TMDI calculation is not implemented for per-person.");
            }
            if (!consumptionIntakeUnit.IsPerPerson()) {
                throw new NotImplementedException("IEDI and TMDI calculation is not implemented for per bodyweight consumptions.");
            }
            if (consumptionIntakeUnit.GetConsumptionUnit() != ConsumptionUnit.g) {
                throw new NotImplementedException("IEDI and TMDI calculation is only implemented for consumptions in grams.");
            }
            var result = compute(
                population,
                substances,
                singleValueConsumptionModels,
                singleValueConcentrationModels,
                occurrenceFractions,
                consumptionIntakeUnit,
                concentrationUnit,
                consumptionBodyWeightUnit,
                targetUnit
            );
            return result.Cast<ISingleValueDietaryExposure>().ToList();
        }

        /// <summary>
        /// Calculate chronic estimates.
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
        ///
        /// <returns></returns>
        protected virtual ICollection<ChronicSingleValueDietaryExposureResult> compute(
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
            /// IEDI, TMDI consumption unit is in grams
            var consumptionUnit = ConsumptionUnit.g;

            // Unit conversion factor for concentrations
            var conversionFactorConcentrationMassUnit = concentrationUnit.GetConcentrationMassUnit().GetMultiplicationFactor(ConcentrationMassUnitConverter.FromConsumptionUnit(consumptionUnit));
            var conversionFactorConcentrationAmountUnit = concentrationUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, double.NaN);
            var concentrationUnitConversionFactor = conversionFactorConcentrationAmountUnit / conversionFactorConcentrationMassUnit;

            var results = new List<ChronicSingleValueDietaryExposureResult>();
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
                    nominalBodyWeight = bodyWeightUnitConversionFactor * population.NominalBodyWeight;
                } else {
                    throw new Exception($"No body weight specified for food consumption of {singleValueConsumptionModel.Food.Name} and no population bodyweight to fall back on.");
                }

                var food = singleValueConsumptionModel.Food;
                var baseFood = food.BaseFood ?? food;
                foreach (var substance in substances) {
                    // Consumption
                    var consumptionUnitConversionFactor = consumptionIntakeUnit.GetConsumptionUnit().GetMultiplicationFactor(consumptionUnit);
                    var meanConsumption = consumptionUnitConversionFactor * singleValueConsumptionModel.MeanConsumption;

                    // Concentration
                    singleValueConcentrationModels.TryGetValue((baseFood, substance), out var singleValueConcentrationModel);
                    var mrl = singleValueConcentrationModel?.Mrl ?? double.NaN;
                    mrl *= concentrationUnitConversionFactor;
                    (var concentration, var concentrationValueType) = getConcentrationValue(singleValueConcentrationModel, mrl);
                    concentration *= concentrationUnitConversionFactor;
                    if (double.IsNaN(concentration)) {
                        // If no concentration available, skip
                        continue;
                    }

                    // Processing factor
                    var processingTypes = singleValueConsumptionModel.ProcessingTypes;
                    var processingFactor = (processingTypes?.Count > 0)
                        ? _processingFactors?.GetNominalProcessingFactor(baseFood, substance, processingTypes.Last()) ?? double.NaN
                        : double.NaN;

                    // Occurrence frequency
                    var useFrequency = getOccurrenceFraction(baseFood, substance, occurrenceFractions)?.OccurrenceFrequency ?? 1;

                    // Exposure
                    var pf = _isApplyProcessingFactors && !double.IsNaN(processingFactor) ? processingFactor : 1D;
                    var exposure = meanConsumption * pf * concentration * useFrequency / nominalBodyWeight;

                    var record = new ChronicSingleValueDietaryExposureResult() {
                        Food = baseFood,
                        ProcessingType = processingTypes?.LastOrDefault(),
                        Substance = substance,
                        CalculationMethod = CalculationMethod,
                        MeanConsumption = meanConsumption,
                        ConcentrationValue = concentration,
                        ConcentrationValueType = concentrationValueType,
                        ProcessingFactor = processingFactor,
                        Exposure = exposure,
                        OccurrenceFraction = useFrequency,
                        BodyWeight = nominalBodyWeight
                    };
                    results.Add(record);
                }
            }
            return results;
        }

        /// <summary>
        /// Get concentration based om MRL setting
        /// </summary>
        /// <param name="singleValueConcentrationModel"></param>
        /// <returns></returns>
        protected (double, ConcentrationValueType) getConcentrationValue(
            SingleValueConcentrationModel singleValueConcentrationModel,
            double mrl
        ) {
            if (singleValueConcentrationModel == null) {
                return (double.NaN, ConcentrationValueType.Undefined);
            } else if (_isMrlSetting) {
                return (mrl, ConcentrationValueType.MRL);
            } else {
                if (!double.IsNaN(singleValueConcentrationModel.GetPercentile(50))) {
                    return (singleValueConcentrationModel.GetPercentile(50), ConcentrationValueType.MedianConcentration);
                } else {
                    return (singleValueConcentrationModel.Loq, ConcentrationValueType.LOQ);
                }
            }
        }

        private OccurrenceFraction getOccurrenceFraction(
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
