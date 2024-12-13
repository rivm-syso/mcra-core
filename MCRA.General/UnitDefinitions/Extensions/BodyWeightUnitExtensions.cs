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
            return targetUnit switch {
                BodyWeightUnit.kg => 3,
                BodyWeightUnit.g => (double)0,
                _ => throw new Exception($"Unknown body weight unit type {targetUnit}"),
            };
        }
    }
}
