namespace MCRA.General.UnitDefinitions.Defaults {
    /// <summary>
    /// Defines MCRA system defaults for units.
    /// </summary>
    public static class SystemUnits {
        // Concentration units
        public static readonly ConcentrationUnit DefaultConcentrationUnit = ConcentrationUnit.ugPerg;
        public static readonly ConcentrationUnit DefaultSingleValueConcentrationUnit = ConcentrationUnit.mgPerKg;
        public static readonly AirConcentrationUnit DefaultAirConcentrationUnit = AirConcentrationUnit.ugPerm3;

        public static readonly ConcentrationUnit DefaultConsumerProductConcentrationUnit = DefaultSingleValueConcentrationUnit;
        public static readonly ConcentrationUnit DefaultDustConcentrationUnit = DefaultConcentrationUnit;
        public static readonly ConcentrationUnit DefaultSoilConcentrationUnit = DefaultConcentrationUnit;

        // Dose units
        public static readonly DoseUnit DefaultExternalHazardDoseUnit = DoseUnit.ugPerKgBWPerDay;
    }
}
