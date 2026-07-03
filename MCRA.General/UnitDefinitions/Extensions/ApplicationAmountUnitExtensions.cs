namespace MCRA.General {
    public static class ApplicationAmountUnitExtensions {

        public static bool IsVolumeUnit(this ApplicationAmountUnit unit) {
            return unit switch {
                ApplicationAmountUnit.nL => true,
                ApplicationAmountUnit.uL => true,
                ApplicationAmountUnit.mL => true,
                ApplicationAmountUnit.cL => true,
                ApplicationAmountUnit.dL => true,
                ApplicationAmountUnit.L => true,
                _ => false
            };
        }

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

        public static double GetMultiplicationFactor(
            this ApplicationAmountUnit unit,
            ConcentrationMassUnit target
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
                ApplicationAmountUnit.L => 0D,
                ApplicationAmountUnit.dL => -1D,
                ApplicationAmountUnit.cL => -2D,
                ApplicationAmountUnit.mL => -3D,
                ApplicationAmountUnit.uL => -6D,
                ApplicationAmountUnit.nL => -9D,
                _ => throw new Exception($"Failed to extract log 10 multiplier for consumer product application unit {unit}."),
            };
        }

        private static double getLog10MetricPrefixMultiplier(ConcentrationMassUnit unit) {
            return unit switch {
                ConcentrationMassUnit.Kilograms => 3D,
                ConcentrationMassUnit.Grams=> 0D,
                ConcentrationMassUnit.MilliGrams=>-3D,
                ConcentrationMassUnit.Liter => 0D,
                ConcentrationMassUnit.Deciliter => -1D,
                ConcentrationMassUnit.Centiliter => -2D,
                ConcentrationMassUnit.Milliliter => -3D,
                ConcentrationMassUnit.Microliter => -6D,
                ConcentrationMassUnit.Nanoliter => -9D,
                _ => throw new Exception($"Failed to extract log 10 multiplier for consumer product application unit {unit}."),
            };
        }
    }
}
