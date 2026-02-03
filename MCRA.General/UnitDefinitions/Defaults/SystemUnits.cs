namespace MCRA.General.UnitDefinitions.Defaults {
    /// <summary>
    /// Defines MCRA system defaults for units.
    /// </summary>
    public static class SystemUnits {

        public static readonly ConcentrationUnit DefaultConcentrationUnit = ConcentrationUnit.ugPerg;
        public static readonly ConcentrationUnit DefaultConsumerProductConcentrationUnit = ConcentrationUnit.ugPerKg;
        public static readonly ConcentrationUnit DefaultDustConcentrationUnit = ConcentrationUnit.ugPerg;
        public static readonly ConcentrationUnit DefaultSingleValueConcentrationUnit = ConcentrationUnit.mgPerKg;
        public static readonly ConcentrationUnit DefaultSoilConcentrationUnit = ConcentrationUnit.ugPerg;

        public static readonly AirConcentrationUnit DefaultAirConcentrationUnit = AirConcentrationUnit.ugPerm3;
    }
}
