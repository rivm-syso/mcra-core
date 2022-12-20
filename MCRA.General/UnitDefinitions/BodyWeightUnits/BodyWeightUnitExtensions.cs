using System;

namespace MCRA.General {
    public static class BodyWeightUnitExtensions {
        /// <summary>
        /// Returns the multiplication factor to convert body weights expressed in the
        /// input body weight unit to body weights expressed in the target unit.
        /// </summary>
        /// <param name="bodyWeightUnit"></param>
        /// <param name="targetUnit"></param>
        /// <returns></returns>
        public static double GetBodyWeightUnitMultiplier(this BodyWeightUnit bodyWeightUnit, BodyWeightUnit targetUnit) {
            var bodyWeightMultiplier = GetLog10BodyWeightUnitMultiplier(bodyWeightUnit);
            var targetMultiplier = GetLog10BodyWeightUnitMultiplier(targetUnit);
            return Math.Pow(10, bodyWeightMultiplier - targetMultiplier);
        }

        /// <summary>
        /// Gets the log10 of the multiplier needed to convert the specified body weight unit
        /// to the standard unit of mass; grams.
        /// </summary>
        /// <param name="targetUnit"></param>
        /// <returns></returns>
        public static double GetLog10BodyWeightUnitMultiplier(BodyWeightUnit targetUnit) {
            switch (targetUnit) {
                case BodyWeightUnit.kg:
                    return 3;
                case BodyWeightUnit.g:
                    return 0;
                default:
                    throw new Exception($"Unknown body weight unit type {targetUnit}");
            }
        }
    }
}
