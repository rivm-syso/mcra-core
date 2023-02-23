namespace MCRA.General {
    public static class ConsumptionIntakeUnitExtensions {

        /// <summary>
        /// Gets the multiplication factor to convert the specified consumption intake unit
        /// to the specified target consumption intake unit. When conversion is needed to/from
        /// per BW / per person, then the specified nominal bodyweight is used to make this
        /// conversion.
        /// </summary>
        /// <param name="consumptionIntakeUnit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="nominalBodyWeight"></param>
        /// <returns></returns>
        public static double GetTargetUnitConversionFactor(
            this ConsumptionIntakeUnit consumptionIntakeUnit,
            ConsumptionIntakeUnit targetUnit,
            double nominalBodyWeight
        ) {
            var bodyWeightConversionFactor = 1D;
            var amountConversionFactor = consumptionIntakeUnit.GetConsumptionUnit().GetMultiplicationFactor(targetUnit.GetConsumptionUnit());
            if (consumptionIntakeUnit.IsPerPerson() && !targetUnit.IsPerPerson()) {
                var bodyWeightUnitMultiplier = BodyWeightUnit.kg.GetBodyWeightUnitMultiplier(targetUnit.GetBodyWeightUnit());
                bodyWeightConversionFactor = 1 / (bodyWeightUnitMultiplier * nominalBodyWeight);
            } else if (!consumptionIntakeUnit.IsPerPerson() && targetUnit.IsPerPerson()) {
                var bodyWeightUnitMultiplier = consumptionIntakeUnit.GetBodyWeightUnit().GetBodyWeightUnitMultiplier(BodyWeightUnit.kg);
                bodyWeightConversionFactor = bodyWeightUnitMultiplier * nominalBodyWeight;
            } else if (!consumptionIntakeUnit.IsPerPerson() && !targetUnit.IsPerPerson()) {
                bodyWeightConversionFactor = consumptionIntakeUnit.GetBodyWeightUnit().GetBodyWeightUnitMultiplier(targetUnit.GetBodyWeightUnit());
            }
            return bodyWeightConversionFactor * amountConversionFactor;
        }

        /// <summary>
        /// Returns true when the consumption intake unit is a per-person unit, otherwise
        /// it returns false (associated with a per BW unit).
        /// </summary>
        /// <param name="consumptionIntakeUnit"></param>
        /// <returns></returns>
        public static bool IsPerPerson(this ConsumptionIntakeUnit consumptionIntakeUnit) {
            switch (consumptionIntakeUnit) {
                case ConsumptionIntakeUnit.gPerKgBWPerDay:
                    return false;
                case ConsumptionIntakeUnit.gPerDay:
                    return true;
                default:
                    throw new NotImplementedException($"IsPerPerson getter not implemented for consumption intake unit {consumptionIntakeUnit}.");
            }
        }

        /// <summary>
        /// Gets the consumption unit part of the consumption intake unit.
        /// </summary>
        /// <param name="consumptionIntakeUnit"></param>
        /// <returns></returns>
        public static ConsumptionUnit GetConsumptionUnit(this ConsumptionIntakeUnit consumptionIntakeUnit) {
            switch (consumptionIntakeUnit) {
                case ConsumptionIntakeUnit.gPerKgBWPerDay:
                case ConsumptionIntakeUnit.gPerDay:
                    return ConsumptionUnit.g;
                default:
                    throw new NotImplementedException($"No consumption amount unit specified for consumption intake unit {consumptionIntakeUnit}.");
            }
        }

        /// <summary>
        /// Gets the body weight unit from the consumption intake unit. If the consumption intake
        /// unit is per person, then kg is returned as a default.
        /// </summary>
        /// <param name="consumptionIntakeUnit"></param>
        /// <returns></returns>
        public static BodyWeightUnit GetBodyWeightUnit(this ConsumptionIntakeUnit consumptionIntakeUnit) {
            switch (consumptionIntakeUnit) {
                case ConsumptionIntakeUnit.gPerKgBWPerDay:
                case ConsumptionIntakeUnit.gPerDay:
                    return BodyWeightUnit.kg;
                default:
                    throw new NotImplementedException($"No bodyweight unit specified for consumption intake unit {consumptionIntakeUnit}.");
            }
        }
    }
}
