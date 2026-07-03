namespace MCRA.General.UnitDefinitions.Defaults {
    /// <summary>
    /// Defines MCRA system defaults for units.
    /// </summary>
    public static class SystemUnits {
        // Concentration units
        public static readonly ConcentrationUnit DefaultConcentrationUnit = ConcentrationUnit.ugPerg;
        public static readonly ConcentrationUnit DefaultSingleValueConcentrationUnit = ConcentrationUnit.mgPerKg;
        public static readonly AirConcentrationUnit DefaultAirConcentrationUnit = AirConcentrationUnit.ugPerm3;

        public static readonly ConcentrationUnit DefaultDustConcentrationUnit = DefaultConcentrationUnit;
        public static readonly ConcentrationUnit DefaultSoilConcentrationUnit = DefaultConcentrationUnit;
        public static readonly ConsumerProductConcentrationUnit DefaultConsumerProductConcentrationUnit = ConsumerProductConcentrationUnit.mgPerKg;
        public static readonly ConsumerProductConcentrationUnit DefaultConsumerProductConcentrationUnitPerVolume = ConsumerProductConcentrationUnit.ugPermL;
        public static readonly ConsumerProductConcentrationUnit DefaultConsumerProductConcentrationUnitPerUnit = ConsumerProductConcentrationUnit.mgPerUnit;

        public static readonly ApplicationAmountUnit DefaultApplicationAmountUnit= ApplicationAmountUnit.g;
        public static readonly ApplicationAmountUnit DefaultApplicationAmountUnitVolume = ApplicationAmountUnit.mL;

        // Dose units
        public static readonly DoseUnit DefaultExternalHazardDoseUnit = DoseUnit.ugPerKgBWPerDay;
    }
}
