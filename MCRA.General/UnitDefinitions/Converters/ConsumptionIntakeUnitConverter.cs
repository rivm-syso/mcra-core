namespace MCRA.General {
    public partial class ConsumptionIntakeUnitConverter : UnitConverterBase<ConsumptionIntakeUnit> {
        /// <summary>
        /// Create a consumption intake unit from a consumption unit.
        /// </summary>
        /// <param name="consumptionUnit"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public static ConsumptionIntakeUnit FromConsumptionUnit(ConsumptionUnit consumptionUnit, bool isPerPerson) {
            if (isPerPerson) {
                switch (consumptionUnit) {
                    case ConsumptionUnit.g:
                        return ConsumptionIntakeUnit.gPerDay;
                    default:
                        throw new NotImplementedException($"Cannot find a per person consumption unit for consumption amount unit {consumptionUnit}");
                }
            } else {
                switch (consumptionUnit) {
                    case ConsumptionUnit.g:
                        return ConsumptionIntakeUnit.gPerKgBWPerDay;
                    default:
                        throw new NotImplementedException($"Cannot find a per kg/BW consumption unit for consumption amount unit {consumptionUnit}");
                }
            }
        }
    }
}
