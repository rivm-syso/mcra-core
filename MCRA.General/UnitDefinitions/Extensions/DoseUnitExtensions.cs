namespace MCRA.General {
    public static class DoseUnitExtensions {
        /// <summary>
        /// Gets the multiplication factor to align a dose of the specified dose unit with the specified
        /// target unit. This method ignores the time-component of the target unit and assumes that the
        /// target is specified as a concentration (i.e., per kg or per g and not per person).
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="molarMass"></param>
        /// <returns></returns>
        public static double GetDoseAlignmentFactor(this DoseUnit unit, TargetUnit targetUnit, double molarMass) {
            var substanceAmountCorrectionFactor = unit.GetSubstanceAmountUnit().GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, molarMass);
            var concentrationMassCorrectionFactor = unit.GetConcentrationMassUnit().GetMultiplicationFactor(targetUnit.ConcentrationMassUnit);
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor / unit.GetDoseUnitPeriodDivider();
        }

        /// <summary>
        /// Returns the substance amount unit of the dose unit.
        /// </summary>
        /// <param name="doseUnit"></param>
        /// <returns></returns>
        public static SubstanceAmountUnit GetSubstanceAmountUnit(this DoseUnit doseUnit) {
            switch (doseUnit) {
                case DoseUnit.kgPerDay:
                case DoseUnit.kgPerWeek:
                case DoseUnit.kgPerKg:
                case DoseUnit.kgPerL:
                    return SubstanceAmountUnit.Kilograms;
                case DoseUnit.gPerKgBWPerDay:
                case DoseUnit.gPerGBWPerDay:
                case DoseUnit.gPerDay:
                case DoseUnit.gPerKgBWPerWeek:
                case DoseUnit.gPerGBWPerWeek:
                case DoseUnit.gPerWeek:
                case DoseUnit.gPerKg:
                case DoseUnit.gPerL:
                    return SubstanceAmountUnit.Grams;
                case DoseUnit.mgPerKgBWPerDay:
                case DoseUnit.mgPerGBWPerDay:
                case DoseUnit.mgPerDay:
                case DoseUnit.mgPerKgBWPerWeek:
                case DoseUnit.mgPerGBWPerWeek:
                case DoseUnit.mgPerWeek:
                case DoseUnit.mgPerKg:
                case DoseUnit.mgPerL:
                    return SubstanceAmountUnit.Milligrams;
                case DoseUnit.ugPerKgBWPerDay:
                case DoseUnit.ugPerGBWPerDay:
                case DoseUnit.ugPerDay:
                case DoseUnit.ugPerKgBWPerWeek:
                case DoseUnit.ugPerGBWPerWeek:
                case DoseUnit.ugPerWeek:
                case DoseUnit.ugPerKg:
                case DoseUnit.ugPerL:
                    return SubstanceAmountUnit.Micrograms;
                case DoseUnit.ngPerKgBWPerDay:
                case DoseUnit.ngPerGBWPerDay:
                case DoseUnit.ngPerDay:
                case DoseUnit.ngPerKgBWPerWeek:
                case DoseUnit.ngPerGBWPerWeek:
                case DoseUnit.ngPerWeek:
                case DoseUnit.ngPerKg:
                case DoseUnit.ngPerL:
                    return SubstanceAmountUnit.Nanograms;
                case DoseUnit.pgPerKgBWPerDay:
                case DoseUnit.pgPerGBWPerDay:
                case DoseUnit.pgPerDay:
                case DoseUnit.pgPerKgBWPerWeek:
                case DoseUnit.pgPerGBWPerWeek:
                case DoseUnit.pgPerWeek:
                case DoseUnit.pgPerKg:
                case DoseUnit.pgPerL:
                    return SubstanceAmountUnit.Picograms;
                case DoseUnit.fgPerKgBWPerDay:
                case DoseUnit.fgPerGBWPerDay:
                case DoseUnit.fgPerDay:
                case DoseUnit.fgPerKgBWPerWeek:
                case DoseUnit.fgPerGBWPerWeek:
                case DoseUnit.fgPerWeek:
                    return SubstanceAmountUnit.Femtograms;
                case DoseUnit.M:
                case DoseUnit.moles:
                    return SubstanceAmountUnit.Moles;
                case DoseUnit.mM:
                case DoseUnit.mmoles:
                    return SubstanceAmountUnit.Millimoles;
                case DoseUnit.uM:
                case DoseUnit.umoles:
                    return SubstanceAmountUnit.Micromoles;
                case DoseUnit.nM:
                case DoseUnit.nmoles:
                    return SubstanceAmountUnit.Nanomoles;
                default:
                    throw new Exception($"Failed to extract substance amount unit from dose unit {doseUnit}!");
            }
        }


        /// <summary>
        /// Returns the day or week divider of the dose unit. For days = 1; for weeks = 7
        /// </summary>
        /// <param name="doseUnit"></param>
        /// <returns></returns>
        public static double GetDoseUnitPeriodDivider(this DoseUnit doseUnit) {
            switch (doseUnit) {
                case DoseUnit.kgPerWeek:
                case DoseUnit.gPerKgBWPerWeek:
                case DoseUnit.gPerGBWPerWeek:
                case DoseUnit.gPerWeek:
                case DoseUnit.mgPerKgBWPerWeek:
                case DoseUnit.mgPerGBWPerWeek:
                case DoseUnit.mgPerWeek:
                case DoseUnit.ugPerKgBWPerWeek:
                case DoseUnit.ugPerGBWPerWeek:
                case DoseUnit.ugPerWeek:
                case DoseUnit.ngPerKgBWPerWeek:
                case DoseUnit.ngPerGBWPerWeek:
                case DoseUnit.ngPerWeek:
                case DoseUnit.pgPerKgBWPerWeek:
                case DoseUnit.pgPerGBWPerWeek:
                case DoseUnit.pgPerWeek:
                case DoseUnit.fgPerKgBWPerWeek:
                case DoseUnit.fgPerGBWPerWeek:
                case DoseUnit.fgPerWeek:
                    return 7;
                case DoseUnit.kgPerDay:
                case DoseUnit.kgPerKg:
                case DoseUnit.kgPerL:
                case DoseUnit.gPerKgBWPerDay:
                case DoseUnit.gPerGBWPerDay:
                case DoseUnit.gPerDay:
                case DoseUnit.gPerKg:
                case DoseUnit.gPerL:
                case DoseUnit.mgPerKgBWPerDay:
                case DoseUnit.mgPerGBWPerDay:
                case DoseUnit.mgPerDay:
                case DoseUnit.mgPerKg:
                case DoseUnit.mgPerL:
                case DoseUnit.ugPerKgBWPerDay:
                case DoseUnit.ugPerGBWPerDay:
                case DoseUnit.ugPerDay:
                case DoseUnit.ugPerKg:
                case DoseUnit.ugPerL:
                case DoseUnit.ngPerKgBWPerDay:
                case DoseUnit.ngPerGBWPerDay:
                case DoseUnit.ngPerDay:
                case DoseUnit.ngPerKg:
                case DoseUnit.ngPerL:
                case DoseUnit.pgPerKgBWPerDay:
                case DoseUnit.pgPerGBWPerDay:
                case DoseUnit.pgPerDay:
                case DoseUnit.pgPerKg:
                case DoseUnit.pgPerL:
                case DoseUnit.fgPerKgBWPerDay:
                case DoseUnit.fgPerGBWPerDay:
                case DoseUnit.fgPerDay:
                case DoseUnit.M:
                case DoseUnit.moles:
                case DoseUnit.mM:
                case DoseUnit.mmoles:
                case DoseUnit.uM:
                case DoseUnit.umoles:
                case DoseUnit.nM:
                case DoseUnit.nmoles:
                    return 1;
                default:
                    throw new Exception($"Failed to extract substance amount unit from dose unit {doseUnit}!");
            }
        }
        /// <summary>
        /// Returns the unit mass unit of the dose unit.
        /// </summary>
        /// <param name="doseUnit"></param>
        /// <returns></returns>
        public static ConcentrationMassUnit GetConcentrationMassUnit(this DoseUnit doseUnit) {
            switch (doseUnit) {
                case DoseUnit.gPerKgBWPerDay:
                case DoseUnit.mgPerKgBWPerDay:
                case DoseUnit.ugPerKgBWPerDay:
                case DoseUnit.ngPerKgBWPerDay:
                case DoseUnit.pgPerKgBWPerDay:
                case DoseUnit.fgPerKgBWPerDay:
                case DoseUnit.gPerKgBWPerWeek:
                case DoseUnit.mgPerKgBWPerWeek:
                case DoseUnit.ugPerKgBWPerWeek:
                case DoseUnit.ngPerKgBWPerWeek:
                case DoseUnit.pgPerKgBWPerWeek:
                case DoseUnit.fgPerKgBWPerWeek:
                    return ConcentrationMassUnit.Kilograms;
                case DoseUnit.gPerGBWPerDay:
                case DoseUnit.mgPerGBWPerDay:
                case DoseUnit.ugPerGBWPerDay:
                case DoseUnit.ngPerGBWPerDay:
                case DoseUnit.pgPerGBWPerDay:
                case DoseUnit.fgPerGBWPerDay:
                case DoseUnit.gPerGBWPerWeek:
                case DoseUnit.mgPerGBWPerWeek:
                case DoseUnit.ugPerGBWPerWeek:
                case DoseUnit.ngPerGBWPerWeek:
                case DoseUnit.pgPerGBWPerWeek:
                case DoseUnit.fgPerGBWPerWeek:
                    return ConcentrationMassUnit.Grams;
                case DoseUnit.kgPerDay:
                case DoseUnit.gPerDay:
                case DoseUnit.mgPerDay:
                case DoseUnit.ugPerDay:
                case DoseUnit.ngPerDay:
                case DoseUnit.pgPerDay:
                case DoseUnit.fgPerDay:
                case DoseUnit.kgPerWeek:
                case DoseUnit.gPerWeek:
                case DoseUnit.mgPerWeek:
                case DoseUnit.ugPerWeek:
                case DoseUnit.ngPerWeek:
                case DoseUnit.pgPerWeek:
                case DoseUnit.fgPerWeek:
                    return ConcentrationMassUnit.PerUnit;
                case DoseUnit.kgPerKg:
                case DoseUnit.gPerKg:
                case DoseUnit.mgPerKg:
                case DoseUnit.ugPerKg:
                case DoseUnit.ngPerKg:
                case DoseUnit.pgPerKg:
                    return ConcentrationMassUnit.Kilograms;
                case DoseUnit.M:
                case DoseUnit.mM:
                case DoseUnit.uM:
                case DoseUnit.nM:
                    return ConcentrationMassUnit.Kilograms;
                case DoseUnit.moles:
                case DoseUnit.mmoles:
                case DoseUnit.umoles:
                case DoseUnit.nmoles:
                    return ConcentrationMassUnit.PerUnit;
                case DoseUnit.kgPerL:
                case DoseUnit.gPerL:
                case DoseUnit.mgPerL:
                case DoseUnit.ugPerL:
                case DoseUnit.ngPerL:
                case DoseUnit.pgPerL:
                    return ConcentrationMassUnit.Liter;
                default:
                    throw new Exception($"Compartment mass unit not known for dose unit {doseUnit}!");
            }
        }

        /// <summary>
        /// Returns the time scale specification of the dose unit.
        /// </summary>
        /// <param name="doseUnit"></param>
        /// <returns></returns>
        public static TimeScaleUnit GetTimeScaleUnit(this DoseUnit doseUnit) {
            switch (doseUnit) {
                case DoseUnit.gPerKgBWPerDay:
                case DoseUnit.mgPerKgBWPerDay:
                case DoseUnit.ugPerKgBWPerDay:
                case DoseUnit.ngPerKgBWPerDay:
                case DoseUnit.pgPerKgBWPerDay:
                case DoseUnit.fgPerKgBWPerDay:
                case DoseUnit.gPerGBWPerDay:
                case DoseUnit.mgPerGBWPerDay:
                case DoseUnit.ugPerGBWPerDay:
                case DoseUnit.ngPerGBWPerDay:
                case DoseUnit.pgPerGBWPerDay:
                case DoseUnit.fgPerGBWPerDay:
                case DoseUnit.kgPerDay:
                case DoseUnit.gPerDay:
                case DoseUnit.mgPerDay:
                case DoseUnit.ugPerDay:
                case DoseUnit.ngPerDay:
                case DoseUnit.pgPerDay:
                case DoseUnit.fgPerDay:
                    return TimeScaleUnit.PerDay;
                case DoseUnit.gPerKgBWPerWeek:
                case DoseUnit.mgPerKgBWPerWeek:
                case DoseUnit.ugPerKgBWPerWeek:
                case DoseUnit.ngPerKgBWPerWeek:
                case DoseUnit.pgPerKgBWPerWeek:
                case DoseUnit.fgPerKgBWPerWeek:
                case DoseUnit.gPerGBWPerWeek:
                case DoseUnit.mgPerGBWPerWeek:
                case DoseUnit.ugPerGBWPerWeek:
                case DoseUnit.ngPerGBWPerWeek:
                case DoseUnit.pgPerGBWPerWeek:
                case DoseUnit.fgPerGBWPerWeek:
                case DoseUnit.kgPerWeek:
                case DoseUnit.gPerWeek:
                case DoseUnit.mgPerWeek:
                case DoseUnit.ugPerWeek:
                case DoseUnit.ngPerWeek:
                case DoseUnit.pgPerWeek:
                case DoseUnit.fgPerWeek:
                    throw new Exception("Cannot get time scale unit for per-week doses.");
                case DoseUnit.kgPerKg:
                case DoseUnit.gPerKg:
                case DoseUnit.mgPerKg:
                case DoseUnit.ugPerKg:
                case DoseUnit.ngPerKg:
                case DoseUnit.pgPerKg:
                case DoseUnit.kgPerL:
                case DoseUnit.gPerL:
                case DoseUnit.mgPerL:
                case DoseUnit.ugPerL:
                case DoseUnit.ngPerL:
                case DoseUnit.pgPerL:
                case DoseUnit.M:
                case DoseUnit.mM:
                case DoseUnit.uM:
                case DoseUnit.nM:
                case DoseUnit.moles:
                case DoseUnit.mmoles:
                case DoseUnit.umoles:
                case DoseUnit.nmoles:
                    return TimeScaleUnit.Unspecified;
                default:
                    throw new Exception($"Time scale unit not known for dose unit {doseUnit}!");
            }
        }
    }
}
