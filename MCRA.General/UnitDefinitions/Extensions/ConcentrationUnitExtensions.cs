namespace MCRA.General {
    public static class ConcentrationUnitExtensions {
        /// <summary>
        /// Gets the multiplication factor to align a concentration of the specified concentration unit with
        /// the specified target unit. This method ignores the time-component of the target unit and assumes
        /// that the target is specified as a concentration (i.e., per kg or per g and not per person).
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="molarMass"></param>
        /// <returns></returns>
        public static double GetConcentrationAlignmentFactor(this ConcentrationUnit unit, TargetUnit targetUnit, double molarMass) {
            var substanceAmountCorrectionFactor = unit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, molarMass);
            var concentrationMassCorrectionFactor = unit.GetConcentrationMassUnit().GetMultiplicationFactor(targetUnit.ConcentrationMassUnit);
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor;
        }

        /// <summary>
        /// Gets the multiplication factor to align a concentration of the specified concentration unit with
        /// the specified target concentration unit.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="molarMass"></param>
        /// <returns></returns>
        public static double GetConcentrationAlignmentFactor(this ConcentrationUnit unit, ConcentrationUnit targetUnit, double molarMass) {
            var substanceAmountCorrectionFactor = unit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.GetSubstanceAmountUnit(), molarMass);
            var concentrationMassCorrectionFactor = unit.GetConcentrationMassUnit().GetMultiplicationFactor(targetUnit.GetConcentrationMassUnit());
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor;
        }

        /// <summary>
        /// Returns the multiplication factor to convert concentrations expressed in the
        /// concentration unit to concentrations expressed in the target unit.
        /// </summary>
        /// <param name="concentrationUnit"></param>
        /// <param name="targetUnit"></param>
        /// <returns></returns>
        public static double GetConcentrationUnitMultiplier(this ConcentrationUnit concentrationUnit, ConcentrationUnit targetUnit) {
            var concentrationMultiplier = GetLog10ConcentrationAmountMultiplier(concentrationUnit);
            var targetMultiplier = GetLog10ConcentrationAmountMultiplier(targetUnit);
            return Math.Pow(10, concentrationMultiplier - targetMultiplier);
        }

        /// <summary>
        /// Returns the log10 of the substance amount multiplier of the concentration unit.
        /// I.e., kg = 3, g = 0, mg = -3, etc.
        /// </summary>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public static double GetLog10ConcentrationAmountMultiplier(ConcentrationUnit concentrationUnit) {
            var log10Multiplier = 0D;
            switch (concentrationUnit) {
                case ConcentrationUnit.kgPerKg:
                case ConcentrationUnit.kgPerL:
                    log10Multiplier = 3;
                    break;
                case ConcentrationUnit.gPerKg:
                case ConcentrationUnit.gPerL:
                    log10Multiplier = 0;
                    break;
                case ConcentrationUnit.mgPerdL:
                    log10Multiplier = -2;
                    break;
                case ConcentrationUnit.mgPerKg:
                case ConcentrationUnit.mgPerL:
                case ConcentrationUnit.ugPermL:
                case ConcentrationUnit.ugPerg:
                    log10Multiplier = -3;
                    break;
                case ConcentrationUnit.ugPerKg:
                case ConcentrationUnit.ugPerL:
                case ConcentrationUnit.ngPermL:
                case ConcentrationUnit.ngPerg:
                    log10Multiplier = -6;
                    break;
                case ConcentrationUnit.ngPerKg:
                case ConcentrationUnit.ngPerL:
                    log10Multiplier = -9;
                    break;
                case ConcentrationUnit.pgPerKg:
                case ConcentrationUnit.pgPerL:
                    log10Multiplier = -12;
                    break;
                default:
                    throw new Exception($"Unknown concentration type {concentrationUnit}");
            }
            return log10Multiplier;
        }

        /// <summary>
        /// Returns the substance amount unit of the concentration unit.
        /// </summary>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public static SubstanceAmountUnit GetSubstanceAmountUnit(this ConcentrationUnit concentrationUnit) {
            switch (concentrationUnit) {
                case ConcentrationUnit.kgPerKg:
                case ConcentrationUnit.kgPerL:
                    return SubstanceAmountUnit.Kilograms;
                case ConcentrationUnit.gPerKg:
                case ConcentrationUnit.gPerL:
                    return SubstanceAmountUnit.Grams;
                case ConcentrationUnit.mgPerKg:
                case ConcentrationUnit.mgPerL:
                case ConcentrationUnit.mgPerdL:
                    return SubstanceAmountUnit.Milligrams;
                case ConcentrationUnit.ugPerKg:
                case ConcentrationUnit.ugPerL:
                case ConcentrationUnit.ugPermL:
                case ConcentrationUnit.ugPerg:
                    return SubstanceAmountUnit.Micrograms;
                case ConcentrationUnit.ngPerKg:
                case ConcentrationUnit.ngPerL:
                case ConcentrationUnit.ngPermL:
                case ConcentrationUnit.ngPerg:
                    return SubstanceAmountUnit.Nanograms;
                case ConcentrationUnit.pgPerKg:
                case ConcentrationUnit.pgPerL:
                    return SubstanceAmountUnit.Picograms;
                default:
                    throw new Exception($"Failed to extract substance amount unit from concentration unit {concentrationUnit}!");
            }
        }

        /// <summary>
        /// Returns the unit mass unit of the concentration unit.
        /// </summary>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public static ConcentrationMassUnit GetConcentrationMassUnit(this ConcentrationUnit concentrationUnit) {
            switch (concentrationUnit) {
                case ConcentrationUnit.kgPerKg:
                case ConcentrationUnit.gPerKg:
                case ConcentrationUnit.mgPerKg:
                case ConcentrationUnit.ugPerKg:
                case ConcentrationUnit.ngPerKg:
                case ConcentrationUnit.pgPerKg:
                    return ConcentrationMassUnit.Kilograms;
                case ConcentrationUnit.kgPerL:
                case ConcentrationUnit.gPerL:
                case ConcentrationUnit.mgPerL:
                case ConcentrationUnit.ugPerL:
                case ConcentrationUnit.ngPerL:
                case ConcentrationUnit.pgPerL:
                    return ConcentrationMassUnit.Liter;
                case ConcentrationUnit.ugPermL:
                case ConcentrationUnit.ngPermL:
                    return ConcentrationMassUnit.Milliliter;
                case ConcentrationUnit.mgPerdL:
                    return ConcentrationMassUnit.Deciliter;
                case ConcentrationUnit.ngPerg:
                case ConcentrationUnit.ugPerg:
                    return ConcentrationMassUnit.Grams;
                default:
                    throw new Exception($"Compartment mass unit not known for intake unit {concentrationUnit}!");
            }
        }
    }
}
