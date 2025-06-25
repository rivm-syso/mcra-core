namespace MCRA.General {
    public static class ApplicationAmountUnitExtensions {

        /// <summary>
        /// Returns the multiplication factor to convert a consumer product application amount of the specified unit
        /// to its equivalent amount expressed as the target unit.
        /// </summary>
        public static double GetMultiplicationFactor(
            this ApplicationAmountUnit unit,
            ApplicationAmountUnit target
        ) {
            var unitMultiplier = getLog10MetricPrefixMultiplier(unit);
            var targetMultiplier = getLog10MetricPrefixMultiplier(target);
            return Math.Pow(10, unitMultiplier - targetMultiplier);
        }

        private static double getLog10MetricPrefixMultiplier(ApplicationAmountUnit unit) {
            return unit switch {
                ApplicationAmountUnit.kg => 3D,
                ApplicationAmountUnit.g => 0D,
                ApplicationAmountUnit.mg => -3D,
                _ => throw new Exception($"Failed to extract log 10 multiplier for consumer product application unit {unit}."),
            };
        }
    }
}
