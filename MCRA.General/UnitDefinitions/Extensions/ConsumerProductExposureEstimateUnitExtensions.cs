namespace MCRA.General {

    public static class ConsumerProductExposureEstimateUnitExtensions {

        /// <summary>
        /// Returns whether the exposure unit is a per body weight exposure unit or not.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static bool IsPerBodyWeight(this ConsumerProductExposureEstimateUnit exposureUnit) {
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
        public static ConcentrationMassUnit GetConcentrationMassUnit(this ConsumerProductExposureEstimateUnit exposureUnit) {
            switch (exposureUnit) {
                case ConsumerProductExposureEstimateUnit.mgPerKgBWPerDay:
                case ConsumerProductExposureEstimateUnit.ugPerKgBWPerDay:
                case ConsumerProductExposureEstimateUnit.ngPerKgBWPerDay:
                    return ConcentrationMassUnit.Kilograms;
                default:
                    throw new Exception($"Compartment mass unit not known for exposure unit {exposureUnit}!");
            }
        }

        /// <summary>
        /// Returns the time scale specification of the exposure unit.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static TimeScaleUnit GetTimeScale(this ConsumerProductExposureEstimateUnit exposureUnit) {
            switch (exposureUnit) {
                case ConsumerProductExposureEstimateUnit.mgPerKgBWPerDay:
                case ConsumerProductExposureEstimateUnit.ugPerKgBWPerDay:
                case ConsumerProductExposureEstimateUnit.ngPerKgBWPerDay:
                    return TimeScaleUnit.PerDay;
                default:
                    throw new Exception($"Time scale unit not known for exposure unit {exposureUnit}!");
            }
        }

        /// <summary>
        /// Returns the substance amount unit of the exposure unit.
        /// </summary>
        /// <param name="exposureUnit"></param>
        /// <returns></returns>
        public static SubstanceAmountUnit GetSubstanceAmountUnit(this ConsumerProductExposureEstimateUnit exposureUnit) {
            switch (exposureUnit) {
                case ConsumerProductExposureEstimateUnit.mgPerKgBWPerDay:
                    return SubstanceAmountUnit.Milligrams;
                case ConsumerProductExposureEstimateUnit.ugPerKgBWPerDay:
                    return SubstanceAmountUnit.Micrograms;
                case ConsumerProductExposureEstimateUnit.ngPerKgBWPerDay:
                    return SubstanceAmountUnit.Nanograms;
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
        public static double GetConcentrationUnitMultiplier(ConsumerProductExposureEstimateUnit exposureUnit, ConcentrationUnit targetUnit) {
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
            this ConsumerProductExposureEstimateUnit exposureUnit,
            ExposureUnitTriple targetUnit,
            double unitWeight,
            double molarMass = double.NaN
        ) {
            var bodyWeightUnitMultiplier = exposureUnit.GetConcentrationMassUnit().GetMultiplicationFactor(targetUnit.ConcentrationMassUnit, unitWeight);
            var concentrationAmountUnitMultiplier = exposureUnit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, molarMass);
            return concentrationAmountUnitMultiplier / bodyWeightUnitMultiplier;
        }

        private static double getLog10ConcentrationAmountMultiplier(ConsumerProductExposureEstimateUnit exposureUnit) {
            var log10Multiplier = 0D;
            switch (exposureUnit) {
                case ConsumerProductExposureEstimateUnit.mgPerKgBWPerDay:
                    log10Multiplier = -3;
                    break;
                case ConsumerProductExposureEstimateUnit.ugPerKgBWPerDay:
                    log10Multiplier = -6;
                    break;
                case ConsumerProductExposureEstimateUnit.ngPerKgBWPerDay:
                    log10Multiplier = -9;
                    break;
                default:
                    throw new Exception(message: $"Unknown exposure type {exposureUnit}");
            }
            return log10Multiplier;
        }
    }
}
