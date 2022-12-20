using System;

namespace MCRA.General {
    public static class ConsumptionUnitExtensions {

        /// <summary>
        /// Returns the multiplication factor to convert a consumption amount of the specified unit
        /// to its equivalent amount expressed as the target unit.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double GetMultiplicationFactor(
            this ConsumptionUnit unit,
            ConsumptionUnit target
        ) {
            var unitMultiplier = getLog10MetricPrefixMultiplier(unit);
            var targetMultiplier = getLog10MetricPrefixMultiplier(target);
            return Math.Pow(10, unitMultiplier - targetMultiplier);
        }


        private static double getLog10MetricPrefixMultiplier(ConsumptionUnit unit) {
            switch (unit) {
                case ConsumptionUnit.kg:
                    return 3;
                case ConsumptionUnit.g:
                    return 0;
                default:
                    throw new Exception($"Failed to extract log 10 multiplier for consumption amount unit {unit}.");
            }
        }
    }
}
