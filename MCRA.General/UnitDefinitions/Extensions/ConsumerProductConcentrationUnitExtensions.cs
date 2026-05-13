namespace MCRA.General {
    public static class ConsumerProductConcentrationUnitExtensions {

        /// <summary>
        /// Gets the multiplication factor to align a concentration of the specified concentration unit with
        /// the specified target concentration unit.
        /// </summary>
        public static double GetConcentrationAlignmentFactor(
            this ConsumerProductConcentrationUnit unitFrom,
            ConsumerProductConcentrationUnit unitTo,
            double molarMass
        ) {
            var substanceAmountCorrectionFactor = unitFrom.GetSubstanceAmountUnit().GetMultiplicationFactor(unitTo.GetSubstanceAmountUnit(), molarMass);
            var concentrationMassCorrectionFactor = unitFrom.GetConcentrationMassUnit().GetMultiplicationFactor(unitTo.GetConcentrationMassUnit());
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor;
        }


        public static bool IsPerUnit(this ConsumerProductConcentrationUnit unit) {
            return unit.GetConcentrationMassUnit() == ConcentrationMassUnit.PerUnit;
        }

        /// <summary>
        /// Returns the substance amount unit (numerator) of the concentration volume unit.
        /// </summary>
        public static SubstanceAmountUnit GetSubstanceAmountUnit(this ConsumerProductConcentrationUnit consumerProductConcentrationunit) {
            return consumerProductConcentrationunit switch {
                ConsumerProductConcentrationUnit.ugPerg => SubstanceAmountUnit.Micrograms,
                ConsumerProductConcentrationUnit.ngPerg => SubstanceAmountUnit.Nanograms,
                ConsumerProductConcentrationUnit.gPerKg => SubstanceAmountUnit.Grams,
                ConsumerProductConcentrationUnit.mgPerKg => SubstanceAmountUnit.Milligrams,
                ConsumerProductConcentrationUnit.ugPerKg => SubstanceAmountUnit.Micrograms,
                ConsumerProductConcentrationUnit.ngPerKg => SubstanceAmountUnit.Nanograms,
                ConsumerProductConcentrationUnit.pgPerKg => SubstanceAmountUnit.Picograms,
                ConsumerProductConcentrationUnit.gPerL => SubstanceAmountUnit.Grams,
                ConsumerProductConcentrationUnit.mgPerL => SubstanceAmountUnit.Milligrams,
                ConsumerProductConcentrationUnit.ugPerL => SubstanceAmountUnit.Micrograms,
                ConsumerProductConcentrationUnit.ngPerL => SubstanceAmountUnit.Nanograms,
                ConsumerProductConcentrationUnit.pgPerL => SubstanceAmountUnit.Picograms,
                ConsumerProductConcentrationUnit.ugPermL => SubstanceAmountUnit.Micrograms,
                ConsumerProductConcentrationUnit.ngPermL => SubstanceAmountUnit.Nanograms,
                ConsumerProductConcentrationUnit.mgPerdL => SubstanceAmountUnit.Milligrams,
                ConsumerProductConcentrationUnit.gPerUnit => SubstanceAmountUnit.Grams,
                ConsumerProductConcentrationUnit.mgPerUnit => SubstanceAmountUnit.Milligrams,
                ConsumerProductConcentrationUnit.ugPerUnit => SubstanceAmountUnit.Micrograms,
                ConsumerProductConcentrationUnit.ngPerUnit => SubstanceAmountUnit.Nanograms,
                _ => throw new Exception($"Failed to extract substance amount unit from consumer product concentration unit {consumerProductConcentrationunit}!")
            };
        }

        /// <summary>
        /// Returns the unit mass unit (denominator) of the concentration volume unit.
        /// </summary>
        public static ConcentrationMassUnit GetConcentrationMassUnit(this ConsumerProductConcentrationUnit consumerProductConcentrationUnit) {
            return consumerProductConcentrationUnit switch {
                ConsumerProductConcentrationUnit.ugPerg => ConcentrationMassUnit.Grams,
                ConsumerProductConcentrationUnit.ngPerg => ConcentrationMassUnit.Grams,
                ConsumerProductConcentrationUnit.gPerKg => ConcentrationMassUnit.Kilograms,
                ConsumerProductConcentrationUnit.mgPerKg => ConcentrationMassUnit.Kilograms,
                ConsumerProductConcentrationUnit.ugPerKg => ConcentrationMassUnit.Kilograms,
                ConsumerProductConcentrationUnit.ngPerKg => ConcentrationMassUnit.Kilograms,
                ConsumerProductConcentrationUnit.pgPerKg => ConcentrationMassUnit.Kilograms,
                ConsumerProductConcentrationUnit.gPerL => ConcentrationMassUnit.Liter,
                ConsumerProductConcentrationUnit.mgPerL => ConcentrationMassUnit.Liter,
                ConsumerProductConcentrationUnit.ugPerL => ConcentrationMassUnit.Liter,
                ConsumerProductConcentrationUnit.ngPerL => ConcentrationMassUnit.Liter,
                ConsumerProductConcentrationUnit.pgPerL => ConcentrationMassUnit.Liter,
                ConsumerProductConcentrationUnit.ugPermL => ConcentrationMassUnit.Milliliter,
                ConsumerProductConcentrationUnit.ngPermL => ConcentrationMassUnit.Milliliter,
                ConsumerProductConcentrationUnit.mgPerdL => ConcentrationMassUnit.Deciliter,
                ConsumerProductConcentrationUnit.gPerUnit => ConcentrationMassUnit.PerUnit,
                ConsumerProductConcentrationUnit.mgPerUnit => ConcentrationMassUnit.PerUnit,
                ConsumerProductConcentrationUnit.ugPerUnit => ConcentrationMassUnit.PerUnit,
                ConsumerProductConcentrationUnit.ngPerUnit => ConcentrationMassUnit.PerUnit,
                _ => throw new Exception($"Failed to extract concentration mass unit from consumer product concentration unit {consumerProductConcentrationUnit}!")
            };
        }
    }
}
