namespace MCRA.General {

    public static class ExposureUnitExtensions {
        /// <summary>
        /// Returns whether the exposure unit is a per body weight exposure unit or not.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static bool IsPerBodyWeight(this ExternalExposureUnit exposureUnit) {
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
        public static ConcentrationMassUnit GetConcentrationMassUnit(this ExternalExposureUnit exposureUnit) {
            switch (exposureUnit) {
                case ExternalExposureUnit.gPerKgBWPerDay:
                case ExternalExposureUnit.mgPerKgBWPerDay:
                case ExternalExposureUnit.ugPerKgBWPerDay:
                case ExternalExposureUnit.ngPerKgBWPerDay:
                case ExternalExposureUnit.pgPerKgBWPerDay:
                case ExternalExposureUnit.fgPerKgBWPerDay:
                case ExternalExposureUnit.gPerKg:
                case ExternalExposureUnit.mgPerKg:
                case ExternalExposureUnit.ugPerKg:
                case ExternalExposureUnit.ngPerKg:
                case ExternalExposureUnit.pgPerKg:
                case ExternalExposureUnit.fgPerKg:
                    return ConcentrationMassUnit.Kilograms;
                case ExternalExposureUnit.gPerGBWPerDay:
                case ExternalExposureUnit.mgPerGBWPerDay:
                case ExternalExposureUnit.ugPerGBWPerDay:
                case ExternalExposureUnit.ngPerGBWPerDay:
                case ExternalExposureUnit.pgPerGBWPerDay:
                case ExternalExposureUnit.fgPerGBWPerDay:
                    return ConcentrationMassUnit.Grams;
                case ExternalExposureUnit.kgPerDay:
                case ExternalExposureUnit.gPerDay:
                case ExternalExposureUnit.mgPerDay:
                case ExternalExposureUnit.ugPerDay:
                case ExternalExposureUnit.ngPerDay:
                case ExternalExposureUnit.pgPerDay:
                case ExternalExposureUnit.fgPerDay:
                case ExternalExposureUnit.g:
                case ExternalExposureUnit.mg:
                case ExternalExposureUnit.ug:
                case ExternalExposureUnit.ng:
                case ExternalExposureUnit.pg:
                case ExternalExposureUnit.fg:
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
        public static TimeScaleUnit GetTimeScale(this ExternalExposureUnit exposureUnit) {
            switch (exposureUnit) {
                case ExternalExposureUnit.gPerKgBWPerDay:
                case ExternalExposureUnit.mgPerKgBWPerDay:
                case ExternalExposureUnit.ugPerKgBWPerDay:
                case ExternalExposureUnit.ngPerKgBWPerDay:
                case ExternalExposureUnit.pgPerKgBWPerDay:
                case ExternalExposureUnit.fgPerKgBWPerDay:
                case ExternalExposureUnit.gPerGBWPerDay:
                case ExternalExposureUnit.mgPerGBWPerDay:
                case ExternalExposureUnit.ugPerGBWPerDay:
                case ExternalExposureUnit.ngPerGBWPerDay:
                case ExternalExposureUnit.pgPerGBWPerDay:
                case ExternalExposureUnit.fgPerGBWPerDay:
                case ExternalExposureUnit.kgPerDay:
                case ExternalExposureUnit.gPerDay:
                case ExternalExposureUnit.mgPerDay:
                case ExternalExposureUnit.ugPerDay:
                case ExternalExposureUnit.ngPerDay:
                case ExternalExposureUnit.pgPerDay:
                case ExternalExposureUnit.fgPerDay:
                    return TimeScaleUnit.PerDay;
                case ExternalExposureUnit.gPerKg:
                case ExternalExposureUnit.mgPerKg:
                case ExternalExposureUnit.ugPerKg:
                case ExternalExposureUnit.ngPerKg:
                case ExternalExposureUnit.pgPerKg:
                case ExternalExposureUnit.fgPerKg:
                case ExternalExposureUnit.g:
                case ExternalExposureUnit.mg:
                case ExternalExposureUnit.ug:
                case ExternalExposureUnit.ng:
                case ExternalExposureUnit.pg:
                case ExternalExposureUnit.fg:
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
        public static SubstanceAmountUnit GetSubstanceAmountUnit(this ExternalExposureUnit exposureUnit) {
            switch (exposureUnit) {
                case ExternalExposureUnit.kgPerDay:
                    return SubstanceAmountUnit.Kilograms;
                case ExternalExposureUnit.gPerKgBWPerDay:
                case ExternalExposureUnit.gPerGBWPerDay:
                case ExternalExposureUnit.gPerDay:
                case ExternalExposureUnit.gPerKg:
                case ExternalExposureUnit.g:
                    return SubstanceAmountUnit.Grams;
                case ExternalExposureUnit.mgPerKgBWPerDay:
                case ExternalExposureUnit.mgPerGBWPerDay:
                case ExternalExposureUnit.mgPerDay:
                case ExternalExposureUnit.mgPerKg:
                case ExternalExposureUnit.mg:
                    return SubstanceAmountUnit.Milligrams;
                case ExternalExposureUnit.ugPerKgBWPerDay:
                case ExternalExposureUnit.ugPerGBWPerDay:
                case ExternalExposureUnit.ugPerDay:
                case ExternalExposureUnit.ugPerKg:
                case ExternalExposureUnit.ug:
                    return SubstanceAmountUnit.Micrograms;
                case ExternalExposureUnit.ngPerKgBWPerDay:
                case ExternalExposureUnit.ngPerGBWPerDay:
                case ExternalExposureUnit.ngPerDay:
                case ExternalExposureUnit.ngPerKg:
                case ExternalExposureUnit.ng:
                    return SubstanceAmountUnit.Nanograms;
                case ExternalExposureUnit.pgPerKgBWPerDay:
                case ExternalExposureUnit.pgPerGBWPerDay:
                case ExternalExposureUnit.pgPerDay:
                case ExternalExposureUnit.pgPerKg:
                case ExternalExposureUnit.pg:
                    return SubstanceAmountUnit.Picograms;
                case ExternalExposureUnit.fgPerKgBWPerDay:
                case ExternalExposureUnit.fgPerGBWPerDay:
                case ExternalExposureUnit.fgPerDay:
                case ExternalExposureUnit.fgPerKg:
                case ExternalExposureUnit.fg:
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
        public static double GetConcentrationUnitMultiplier(ExternalExposureUnit exposureUnit, ConcentrationUnit targetUnit) {
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
            this ExternalExposureUnit exposureUnit,
            ExposureUnitTriple targetUnit,
            double unitWeight,
            double molarMass = double.NaN
        ) {
            var bodyWeightUnitMultiplier = exposureUnit.GetConcentrationMassUnit().GetMultiplicationFactor(targetUnit.ConcentrationMassUnit, unitWeight);
            var concentrationAmountUnitMultiplier = exposureUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, molarMass);
            return concentrationAmountUnitMultiplier / bodyWeightUnitMultiplier;
        }

        private static double getLog10ConcentrationAmountMultiplier(ExternalExposureUnit exposureUnit) {
            var log10Multiplier = 0D;
            switch (exposureUnit) {
                case ExternalExposureUnit.kgPerDay:
                    log10Multiplier = 3;
                    break;
                case ExternalExposureUnit.gPerKgBWPerDay:
                case ExternalExposureUnit.gPerGBWPerDay:
                case ExternalExposureUnit.gPerDay:
                case ExternalExposureUnit.gPerKg:
                case ExternalExposureUnit.g:
                    log10Multiplier = 0;
                    break;
                case ExternalExposureUnit.mgPerKgBWPerDay:
                case ExternalExposureUnit.mgPerGBWPerDay:
                case ExternalExposureUnit.mgPerDay:
                case ExternalExposureUnit.mgPerKg:
                case ExternalExposureUnit.mg:
                    log10Multiplier = -3;
                    break;
                case ExternalExposureUnit.ugPerKgBWPerDay:
                case ExternalExposureUnit.ugPerGBWPerDay:
                case ExternalExposureUnit.ugPerDay:
                case ExternalExposureUnit.ugPerKg:
                case ExternalExposureUnit.ug:
                    log10Multiplier = -6;
                    break;
                case ExternalExposureUnit.ngPerKgBWPerDay:
                case ExternalExposureUnit.ngPerGBWPerDay:
                case ExternalExposureUnit.ngPerDay:
                case ExternalExposureUnit.ngPerKg:
                case ExternalExposureUnit.ng:
                    log10Multiplier = -9;
                    break;
                case ExternalExposureUnit.pgPerKgBWPerDay:
                case ExternalExposureUnit.pgPerGBWPerDay:
                case ExternalExposureUnit.pgPerDay:
                case ExternalExposureUnit.pgPerKg:
                case ExternalExposureUnit.pg:
                    log10Multiplier = -12;
                    break;
                case ExternalExposureUnit.fgPerKgBWPerDay:
                case ExternalExposureUnit.fgPerGBWPerDay:
                case ExternalExposureUnit.fgPerDay:
                case ExternalExposureUnit.fgPerKg:
                case ExternalExposureUnit.fg:
                    log10Multiplier = -15;
                    break;
                default:
                    throw new Exception(message: $"Unknown exposure type {exposureUnit}");
            }
            return log10Multiplier;
        }
    }
}
