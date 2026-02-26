namespace MCRA.General {

    public static class ConcentrationMassUnitConverter {

        /// <summary>
        /// Gets the conversion factor to translate a concentration mass specified in the given unit
        /// to the specified target unit. When translating between per-unit/per mass units, a unit
        /// weight should be specified.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        /// <param name="unitWeight">
        /// The bodyweight. Should only be specified when precisely one of the units
        /// is specified per-unit. The unit of the bodyweight is assumed to be the
        /// same as the unit specified as concentration.</param>
        /// <returns></returns>
        public static double GetMultiplicationFactor(this ConcentrationMassUnit unit, ConcentrationMassUnit target, double unitWeight = double.NaN) {
            if (unit == ConcentrationMassUnit.Undefined || target == ConcentrationMassUnit.Undefined) {
                throw new Exception($"Cannot convert from {unit} to {target}!");
            } else if (unit == ConcentrationMassUnit.PerUnit && target == ConcentrationMassUnit.PerUnit
                || unit != ConcentrationMassUnit.PerUnit && target != ConcentrationMassUnit.PerUnit) {
                var unitMultiplier = getLog10MetricPrefixMultiplier(unit);
                var targetMultiplier = getLog10MetricPrefixMultiplier(target);
                var multiplier = Math.Pow(10, unitMultiplier - targetMultiplier);
                return multiplier;
            } else if (target == ConcentrationMassUnit.PerUnit) {
                if (!double.IsNaN(unitWeight)) {
                    return 1 / unitWeight;
                } else {
                    throw new Exception($"Conversion from {unit} to {target} requires a specified unit weight!");
                }
            } else if (unit == ConcentrationMassUnit.PerUnit) {
                if (!double.IsNaN(unitWeight)) {
                    return unitWeight;
                } else {
                    throw new Exception($"Conversion from {unit} to {target} requires a specified unit weight!");
                }
            }
            throw new Exception($"Cannot convert from {unit} to {target}!");
        }

        private static double getLog10MetricPrefixMultiplier(ConcentrationMassUnit unit) {
            switch (unit) {
                case ConcentrationMassUnit.Kilograms:
                case ConcentrationMassUnit.Liter:
                    return 3;
                case ConcentrationMassUnit.Deciliter:
                    return 2;
                case ConcentrationMassUnit.Centiliter:
                    return 1;
                case ConcentrationMassUnit.Grams:
                case ConcentrationMassUnit.Milliliter:
                case ConcentrationMassUnit.PerUnit:
                    return 0;
                case ConcentrationMassUnit.MilliGrams:
                    return -3;
                default:
                    throw new Exception($"Failed to extract log 10 multiplier for concentration mass unit {unit}!");
            }
        }

        public static ConcentrationMassUnit FromBodyWeightUnit(BodyWeightUnit bodyWeightUnit) {
            switch (bodyWeightUnit) {
                case BodyWeightUnit.kg:
                    return ConcentrationMassUnit.Kilograms;
                case BodyWeightUnit.g:
                    return ConcentrationMassUnit.Grams;
                default:
                    throw new Exception($"Failed to extract concentration mass unit from bodyweight unit {bodyWeightUnit}!");
            }
        }
        public static ConcentrationMassUnit FromConsumptionUnit(ConsumptionUnit consumptionUnit) {
            switch (consumptionUnit) {
                case ConsumptionUnit.kg:
                    return ConcentrationMassUnit.Kilograms;
                case ConsumptionUnit.g:
                    return ConcentrationMassUnit.Grams;
                default:
                    throw new Exception($"Failed to extract concentration mass unit from consumption unit {consumptionUnit}!");
            }
        }

    }
}
