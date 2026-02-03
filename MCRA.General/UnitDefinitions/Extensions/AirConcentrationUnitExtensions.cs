namespace MCRA.General {
    public static class AirConcentrationUnitExtensions {

        /// <summary>
        /// Gets the multiplication factor to align a concentration of the specified concentration unit with
        /// the specified target concentration unit.
        /// </summary>
        public static double GetConcentrationAlignmentFactor(
            this AirConcentrationUnit unitFrom,
            AirConcentrationUnit unitTo,
            double molarMass
        ) {
            var substanceAmountCorrectionFactor = unitFrom.GetSubstanceAmountUnit().GetMultiplicationFactor(unitTo.GetSubstanceAmountUnit(), molarMass);
            var concentrationMassCorrectionFactor = unitFrom.GetConcentrationVolumeUnit().GetMultiplicationFactor(unitTo.GetConcentrationVolumeUnit());
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor;
        }

        /// <summary>
        /// Returns the substance amount unit (numerator) of the concentration volume unit.
        /// </summary>
        public static SubstanceAmountUnit GetSubstanceAmountUnit(this AirConcentrationUnit concentrationVolumeUnit) {
            switch (concentrationVolumeUnit) {
                case AirConcentrationUnit.ugPerm3:
                case AirConcentrationUnit.ugPerL:
                    return SubstanceAmountUnit.Micrograms;
                
                default:
                    throw new Exception($"Failed to extract substance amount unit from concentration volume unit {concentrationVolumeUnit}!");
            }
        }

        /// <summary>
        /// Returns the unit mass unit (denominator) of the concentration volume unit.
        /// </summary>
        public static VolumeUnit GetConcentrationVolumeUnit(this AirConcentrationUnit concentrationVolumeUnit) {
            switch (concentrationVolumeUnit) {
                case AirConcentrationUnit.ugPerm3:
                    return VolumeUnit.Cubicmeter;
                case AirConcentrationUnit.ugPerL:
                    return VolumeUnit.Liter;
                default:
                    throw new Exception($"Compartment mass unit not known for intake unit {concentrationVolumeUnit}!");
            }
        }
    }
}
