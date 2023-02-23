namespace MCRA.General {

    public static class SubstanceAmountConverter {

        /// <summary>
        /// Returns the multiplication factor to convert a substance amount of the specified unit
        /// to its equivalent amount expressed as the target unit.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        /// <param name="molarMass"></param>
        /// <returns></returns>
        public static double GetMultiplicationFactor(
            this SubstanceAmountUnit unit,
            SubstanceAmountUnit target,
            double molarMass
        ) {
            var unitMultiplier = getLog10MetricPrefixMultiplier(unit);
            var targetMultiplier = getLog10MetricPrefixMultiplier(target);
            var multiplier = Math.Pow(10, unitMultiplier - targetMultiplier);
            if (unit.IsInMoles() && !target.IsInMoles()) {
                return multiplier * molarMass;
            } else if (!unit.IsInMoles() && target.IsInMoles()) {
                return multiplier / molarMass;
            } else {
                return multiplier;
            }
        }

        /// <summary>
        /// Returns true if the substance amount is specified in moles.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsInMoles(this SubstanceAmountUnit unit) {
            switch (unit) {
                case SubstanceAmountUnit.Kilograms:
                case SubstanceAmountUnit.Grams:
                case SubstanceAmountUnit.Milligrams:
                case SubstanceAmountUnit.Micrograms:
                case SubstanceAmountUnit.Nanograms:
                case SubstanceAmountUnit.Picograms:
                case SubstanceAmountUnit.Femtograms:
                    return false;
                case SubstanceAmountUnit.Moles:
                case SubstanceAmountUnit.Millimoles:
                case SubstanceAmountUnit.Micromoles:
                case SubstanceAmountUnit.Nanomoles:
                    return true;
                default:
                    throw new Exception($"Failed to determine whether substance amount unit {unit} is in grams or in moles!");
            }
        }

        /// <summary>
        /// Extracts the substance amount unit (part) from the specified dose unit.
        /// </summary>
        /// <param name="doseUnit"></param>
        /// <returns></returns>
        public static SubstanceAmountUnit FromDoseUnit(DoseUnit doseUnit) {
            switch (doseUnit) {
                case DoseUnit.kgPerDay:
                case DoseUnit.kgPerWeek:
                case DoseUnit.kgPerKg:
                    return SubstanceAmountUnit.Kilograms;
                case DoseUnit.gPerKgBWPerDay:
                case DoseUnit.gPerGBWPerDay:
                case DoseUnit.gPerDay:
                case DoseUnit.gPerKgBWPerWeek:
                case DoseUnit.gPerGBWPerWeek:
                case DoseUnit.gPerWeek:
                case DoseUnit.gPerKg:
                    return SubstanceAmountUnit.Grams;
                case DoseUnit.mgPerKgBWPerDay:
                case DoseUnit.mgPerGBWPerDay:
                case DoseUnit.mgPerDay:
                case DoseUnit.mgPerKgBWPerWeek:
                case DoseUnit.mgPerGBWPerWeek:
                case DoseUnit.mgPerWeek:
                case DoseUnit.mgPerKg:
                    return SubstanceAmountUnit.Milligrams;
                case DoseUnit.ugPerKgBWPerDay:
                case DoseUnit.ugPerGBWPerDay:
                case DoseUnit.ugPerDay:
                case DoseUnit.ugPerKgBWPerWeek:
                case DoseUnit.ugPerGBWPerWeek:
                case DoseUnit.ugPerWeek:
                case DoseUnit.ugPerKg:
                    return SubstanceAmountUnit.Micrograms;
                case DoseUnit.ngPerKgBWPerDay:
                case DoseUnit.ngPerGBWPerDay:
                case DoseUnit.ngPerDay:
                case DoseUnit.ngPerKgBWPerWeek:
                case DoseUnit.ngPerGBWPerWeek:
                case DoseUnit.ngPerWeek:
                case DoseUnit.ngPerKg:
                    return SubstanceAmountUnit.Nanograms;
                case DoseUnit.pgPerKgBWPerDay:
                case DoseUnit.pgPerGBWPerDay:
                case DoseUnit.pgPerDay:
                case DoseUnit.pgPerKgBWPerWeek:
                case DoseUnit.pgPerGBWPerWeek:
                case DoseUnit.pgPerWeek:
                case DoseUnit.pgPerKg:
                    return SubstanceAmountUnit.Picograms;
                case DoseUnit.fgPerKgBWPerDay:
                case DoseUnit.fgPerGBWPerDay:
                case DoseUnit.fgPerDay:
                case DoseUnit.fgPerKgBWPerWeek:
                case DoseUnit.fgPerGBWPerWeek:
                case DoseUnit.fgPerWeek:
                    return SubstanceAmountUnit.Femtograms;
                case DoseUnit.M:
                case DoseUnit.moles:
                    return SubstanceAmountUnit.Moles;
                case DoseUnit.mM:
                case DoseUnit.mmoles:
                    return SubstanceAmountUnit.Millimoles;
                case DoseUnit.uM:
                case DoseUnit.umoles:
                    return SubstanceAmountUnit.Micromoles;
                case DoseUnit.nM:
                case DoseUnit.nmoles:
                    return SubstanceAmountUnit.Nanomoles;
                default:
                    throw new Exception($"Failed to extract substance amount unit from dose unit {doseUnit}!");
            }
        }

        private static double getLog10MetricPrefixMultiplier(SubstanceAmountUnit unit) {
            switch (unit) {
                case SubstanceAmountUnit.Kilograms:
                    return 3;
                case SubstanceAmountUnit.Grams:
                case SubstanceAmountUnit.Moles:
                    return 0;
                case SubstanceAmountUnit.Milligrams:
                case SubstanceAmountUnit.Millimoles:
                    return -3;
                case SubstanceAmountUnit.Micrograms:
                case SubstanceAmountUnit.Micromoles:
                    return -6;
                case SubstanceAmountUnit.Nanograms:
                case SubstanceAmountUnit.Nanomoles:
                    return -9;
                case SubstanceAmountUnit.Picograms:
                    return -12;
                case SubstanceAmountUnit.Femtograms:
                    return -15;
                default:
                    throw new Exception($"Failed to extract log 10 multiplier for substance amount unit {unit}!");
            }
        }
    }
}
