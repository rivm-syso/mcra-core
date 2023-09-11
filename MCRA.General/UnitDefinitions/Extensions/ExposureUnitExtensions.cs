namespace MCRA.General {

    public static class ExposureUnitExtensions {
        /// <summary>
        /// Returns whether the exposure unit is a per body weight exposure unit or not.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static bool IsPerBodyWeight(this ExposureUnit exposureUnit) {
            var massUnit = exposureUnit.GetConcentrationMassUnit();
            switch (massUnit) {
                case ConcentrationMassUnit.Grams:
                case ConcentrationMassUnit.Kilograms:
                    return true;
                case ConcentrationMassUnit.PerUnit:
                    return false;
                default:
                    throw new Exception($"It is not known whether exposure unit {exposureUnit} represents is per bodyweight or per person.");
            }
        }

        /// <summary>
        /// Returns the compartment mass unit.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static ConcentrationMassUnit GetConcentrationMassUnit(this ExposureUnit exposureUnit) {
            switch (exposureUnit) {
                case ExposureUnit.gPerKgBWPerDay:
                case ExposureUnit.mgPerKgBWPerDay:
                case ExposureUnit.ugPerKgBWPerDay:
                case ExposureUnit.ngPerKgBWPerDay:
                case ExposureUnit.pgPerKgBWPerDay:
                case ExposureUnit.fgPerKgBWPerDay:
                case ExposureUnit.gPerKg:
                case ExposureUnit.mgPerKg:
                case ExposureUnit.ugPerKg:
                case ExposureUnit.ngPerKg:
                case ExposureUnit.pgPerKg:
                case ExposureUnit.fgPerKg:
                    return ConcentrationMassUnit.Kilograms;
                case ExposureUnit.gPerGBWPerDay:
                case ExposureUnit.mgPerGBWPerDay:
                case ExposureUnit.ugPerGBWPerDay:
                case ExposureUnit.ngPerGBWPerDay:
                case ExposureUnit.pgPerGBWPerDay:
                case ExposureUnit.fgPerGBWPerDay:
                    return ConcentrationMassUnit.Grams;
                case ExposureUnit.kgPerDay:
                case ExposureUnit.gPerDay:
                case ExposureUnit.mgPerDay:
                case ExposureUnit.ugPerDay:
                case ExposureUnit.ngPerDay:
                case ExposureUnit.pgPerDay:
                case ExposureUnit.fgPerDay:
                case ExposureUnit.g:
                case ExposureUnit.mg:
                case ExposureUnit.ug:
                case ExposureUnit.ng:
                case ExposureUnit.pg:
                case ExposureUnit.fg:
                    return ConcentrationMassUnit.PerUnit;
                default:
                    throw new Exception($"Compartment mass unit not known for exposure unit {exposureUnit}!");
            }
        }

        /// <summary>
        /// Returns the time scale specification of the exposure unit.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static TimeScaleUnit GetTimeScale(this ExposureUnit exposureUnit) {
            switch (exposureUnit) {
                case ExposureUnit.gPerKgBWPerDay:
                case ExposureUnit.mgPerKgBWPerDay:
                case ExposureUnit.ugPerKgBWPerDay:
                case ExposureUnit.ngPerKgBWPerDay:
                case ExposureUnit.pgPerKgBWPerDay:
                case ExposureUnit.fgPerKgBWPerDay:
                case ExposureUnit.gPerGBWPerDay:
                case ExposureUnit.mgPerGBWPerDay:
                case ExposureUnit.ugPerGBWPerDay:
                case ExposureUnit.ngPerGBWPerDay:
                case ExposureUnit.pgPerGBWPerDay:
                case ExposureUnit.fgPerGBWPerDay:
                case ExposureUnit.kgPerDay:
                case ExposureUnit.gPerDay:
                case ExposureUnit.mgPerDay:
                case ExposureUnit.ugPerDay:
                case ExposureUnit.ngPerDay:
                case ExposureUnit.pgPerDay:
                case ExposureUnit.fgPerDay:
                    return TimeScaleUnit.PerDay;
                case ExposureUnit.gPerKg:
                case ExposureUnit.mgPerKg:
                case ExposureUnit.ugPerKg:
                case ExposureUnit.ngPerKg:
                case ExposureUnit.pgPerKg:
                case ExposureUnit.fgPerKg:
                case ExposureUnit.g:
                case ExposureUnit.mg:
                case ExposureUnit.ug:
                case ExposureUnit.ng:
                case ExposureUnit.pg:
                case ExposureUnit.fg:
                    return TimeScaleUnit.SteadyState;
                default:
                    throw new Exception($"Time scale unit not known for exposure unit {exposureUnit}!");
            }
        }

        /// <summary>
        /// Returns the substance amount unit of the exposure unit.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static SubstanceAmountUnit GetSubstanceAmountUnit(this ExposureUnit exposureUnit) {
            switch (exposureUnit) {
                case ExposureUnit.kgPerDay:
                    return SubstanceAmountUnit.Kilograms;
                case ExposureUnit.gPerKgBWPerDay:
                case ExposureUnit.gPerGBWPerDay:
                case ExposureUnit.gPerDay:
                case ExposureUnit.gPerKg:
                case ExposureUnit.g:
                    return SubstanceAmountUnit.Grams;
                case ExposureUnit.mgPerKgBWPerDay:
                case ExposureUnit.mgPerGBWPerDay:
                case ExposureUnit.mgPerDay:
                case ExposureUnit.mgPerKg:
                case ExposureUnit.mg:
                    return SubstanceAmountUnit.Milligrams;
                case ExposureUnit.ugPerKgBWPerDay:
                case ExposureUnit.ugPerGBWPerDay:
                case ExposureUnit.ugPerDay:
                case ExposureUnit.ugPerKg:
                case ExposureUnit.ug:
                    return SubstanceAmountUnit.Micrograms;
                case ExposureUnit.ngPerKgBWPerDay:
                case ExposureUnit.ngPerGBWPerDay:
                case ExposureUnit.ngPerDay:
                case ExposureUnit.ngPerKg:
                case ExposureUnit.ng:
                    return SubstanceAmountUnit.Nanograms;
                case ExposureUnit.pgPerKgBWPerDay:
                case ExposureUnit.pgPerGBWPerDay:
                case ExposureUnit.pgPerDay:
                case ExposureUnit.pgPerKg:
                case ExposureUnit.pg:
                    return SubstanceAmountUnit.Picograms;
                case ExposureUnit.fgPerKgBWPerDay:
                case ExposureUnit.fgPerGBWPerDay:
                case ExposureUnit.fgPerDay:
                case ExposureUnit.fgPerKg:
                case ExposureUnit.fg:
                    return SubstanceAmountUnit.Femtograms;
                default:
                    throw new Exception($"Time scale unit not known for exposure unit {exposureUnit}!");
            }
        }

        /// <summary>
        /// Returns the multiplier needed to align an exposure of the given exposure unit with
        /// the specified target unit.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <param name="targetUnit"></param>
        /// <returns></returns>
        public static double GetConcentrationUnitMultiplier(ExposureUnit exposureUnit, ConcentrationUnit targetUnit) {
            var exposureMultiplier = getLog10ConcentrationAmountMultiplier(exposureUnit);
            var targetMultiplier = ConcentrationUnitExtensions.GetLog10ConcentrationAmountMultiplier(targetUnit);
            return Math.Pow(10, exposureMultiplier - targetMultiplier);
        }

        /// <summary>
        /// Returns the multiplier needed to align an exposure of the given exposure unit with
        /// the specified target unit.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="unitWeight">
        /// The unit weight (e.g., bodyweight). Should only be specified when precisely one of
        /// the units is specified per-unit. The unit of the bodyweight is assumed to be the
        /// same as the unit specified as concentration.</param>
        /// <param name="molarMass"></param>
        /// <returns></returns>
        public static double GetExposureUnitMultiplier(
            this ExposureUnit exposureUnit,
            ExposureUnitTriple targetUnit,
            double unitWeight,
            double molarMass = double.NaN
        ) {
            var bodyWeightUnitMultiplier = exposureUnit.GetConcentrationMassUnit().GetMultiplicationFactor(targetUnit.ConcentrationMassUnit, unitWeight);
            var concentrationAmountUnitMultiplier = exposureUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, molarMass);
            return concentrationAmountUnitMultiplier / bodyWeightUnitMultiplier;
        }

        private static double getLog10ConcentrationAmountMultiplier(ExposureUnit exposureUnit) {
            var log10Multiplier = 0D;
            switch (exposureUnit) {
                case ExposureUnit.kgPerDay:
                    log10Multiplier = 3;
                    break;
                case ExposureUnit.gPerKgBWPerDay:
                case ExposureUnit.gPerGBWPerDay:
                case ExposureUnit.gPerDay:
                case ExposureUnit.gPerKg:
                case ExposureUnit.g:
                    log10Multiplier = 0;
                    break;
                case ExposureUnit.mgPerKgBWPerDay:
                case ExposureUnit.mgPerGBWPerDay:
                case ExposureUnit.mgPerDay:
                case ExposureUnit.mgPerKg:
                case ExposureUnit.mg:
                    log10Multiplier = -3;
                    break;
                case ExposureUnit.ugPerKgBWPerDay:
                case ExposureUnit.ugPerGBWPerDay:
                case ExposureUnit.ugPerDay:
                case ExposureUnit.ugPerKg:
                case ExposureUnit.ug:
                    log10Multiplier = -6;
                    break;
                case ExposureUnit.ngPerKgBWPerDay:
                case ExposureUnit.ngPerGBWPerDay:
                case ExposureUnit.ngPerDay:
                case ExposureUnit.ngPerKg:
                case ExposureUnit.ng:
                    log10Multiplier = -9;
                    break;
                case ExposureUnit.pgPerKgBWPerDay:
                case ExposureUnit.pgPerGBWPerDay:
                case ExposureUnit.pgPerDay:
                case ExposureUnit.pgPerKg:
                case ExposureUnit.pg:
                    log10Multiplier = -12;
                    break;
                case ExposureUnit.fgPerKgBWPerDay:
                case ExposureUnit.fgPerGBWPerDay:
                case ExposureUnit.fgPerDay:
                case ExposureUnit.fgPerKg:
                case ExposureUnit.fg:
                    log10Multiplier = -15;
                    break;
                default:
                    throw new Exception(message: $"Unknown exposure type {exposureUnit}");
            }
            return log10Multiplier;
        }
    }
}
