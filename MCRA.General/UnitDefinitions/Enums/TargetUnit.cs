using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {

    public class TargetUnit {

        public TargetUnit() { }

        public TargetUnit(
            SubstanceAmountUnit substanceAmountUnit,
            ConcentrationMassUnit concentrationMassUnit,
            string compartment,
            TimeScaleUnit timeScaleUnit
        ) {
            SubstanceAmount = substanceAmountUnit;
            TimeScaleUnit = timeScaleUnit;
            ConcentrationMassUnit = concentrationMassUnit;
            Compartment = compartment;
        }

        public TargetUnit(
            ExposureUnit exposureUnit,
            string compartment = null,
            bool? isPerPerson = null
        ) {
            ConcentrationMassUnit = isPerPerson ?? false ? ConcentrationMassUnit.PerUnit : exposureUnit.GetConcentrationMassUnit();
            TimeScaleUnit = exposureUnit.GetTimeScale();
            SubstanceAmount = exposureUnit.GetSubstanceAmountUnit();
            Compartment = compartment;
        }

        /// <summary>
        /// The unit of the substance amounts.
        /// </summary>
        public SubstanceAmountUnit SubstanceAmount { get; set; }

        /// <summary>
        /// The object mass unit. per-unit for absolute amounts (e.g., mg/day), otherwise
        /// a mass unit (e.g., kg for mg/kg BW/day).
        /// </summary>
        public ConcentrationMassUnit ConcentrationMassUnit { get; set; }

        /// <summary>
        /// The time scale. E.g., per day, peak (acute), long term (chronic).
        /// </summary>
        public TimeScaleUnit TimeScaleUnit { get; set; }

        /// <summary>
        /// The target system. May be internal organs, e.g., liver or, in case of external
        /// target exposures, this could be a person/individual.
        /// </summary>
        public string Compartment { get; set; }

        /// <summary>
        /// Sets the timescale of the unit based on the target-level and exposure type.
        /// </summary>
        /// <param name="targetDoseLevel"></param>
        /// <param name="exposureType"></param>
        public void SetTimeScale(TargetLevelType targetDoseLevel, ExposureType exposureType) {
            if (targetDoseLevel == TargetLevelType.External) {
                TimeScaleUnit = TimeScaleUnit.PerDay;
            } else {
                if (exposureType == ExposureType.Chronic) {
                    TimeScaleUnit = TimeScaleUnit.SteadyState;
                } else {
                    TimeScaleUnit = TimeScaleUnit.Peak;
                }
            }
        }

        /// <summary>
        /// Gets whether this is a per-mass unit.
        /// </summary>
        public bool IsPerBodyWeight() {
            return ConcentrationMassUnit != ConcentrationMassUnit.PerUnit;
        }

        /// <summary>
        /// Gets the multiplication factor to align a dose of the specified dose unit with the specified
        /// target unit. This method ignores the time-component of the target unit and requires that
        /// both units should be either per-person units or per kg/g units.
        /// </summary>
        /// <param name="targetUnit"></param>
        /// <param name="molarMass"></param>
        /// <param name="unitWeight"></param>
        /// <returns></returns>
        public double GetAlignmentFactor(TargetUnit targetUnit, double molarMass, double unitWeight) {
            var substanceAmountCorrectionFactor = SubstanceAmount.GetMultiplicationFactor(targetUnit.SubstanceAmount, molarMass);
            var concentrationMassCorrectionFactor = ConcentrationMassUnit.GetMultiplicationFactor(targetUnit.ConcentrationMassUnit, unitWeight);
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor;
        }

        /// <summary>
        /// Returns the short display name of this unit.
        /// </summary>
        /// <returns></returns>
        public string GetShortDisplayName(bool printCompartment) {
            var substanceAmountString = SubstanceAmount.GetShortDisplayName();
            var perUnitString = string.Empty;
            if (ConcentrationMassUnit != ConcentrationMassUnit.PerUnit) {
                if (printCompartment && !string.IsNullOrEmpty(Compartment)) {
                    perUnitString = $"/{ConcentrationMassUnit.GetShortDisplayName()} {Compartment.ToLower()}";
                } else {
                    perUnitString = $"/{ConcentrationMassUnit.GetShortDisplayName()}";
                }
            }
            var perTimeUnitString = string.Empty;
            switch (TimeScaleUnit) {
                case TimeScaleUnit.PerDay:
                    perTimeUnitString = "/day";
                    break;
                case TimeScaleUnit.SteadyState:
                    //perTimeUnitString = " long term";
                    break;
                case TimeScaleUnit.Peak:
                    //perTimeUnitString = " peak";
                    break;
                case TimeScaleUnit.Unspecified:
                default:
                    break;
            }
            return $"{substanceAmountString}{perUnitString}{perTimeUnitString}";
        }

        /// <summary>
        /// Returns the raw display name of this unit.
        /// </summary>
        /// <returns></returns>
        public string GetRawDisplayName() {
            var substanceAmountString = SubstanceAmount.GetShortDisplayName();
            return $"{substanceAmountString}/day";
        }

        /// <summary>
        /// Create a target unit from a dose unit.
        /// </summary>
        /// <param name="doseUnit"></param>
        /// <param name="compartment"></param>
        /// <returns></returns>
        public static TargetUnit FromDoseUnit(DoseUnit doseUnit, string compartment) {
            return new TargetUnit() {
                ConcentrationMassUnit = doseUnit.GetConcentrationMassUnit(),
                TimeScaleUnit = doseUnit.GetTimeScaleUnit(),
                SubstanceAmount = doseUnit.GetSubstanceAmountUnit(),
                Compartment = compartment
            };
        }

        /// <summary>
        /// Gets a target unit from a consumption intake unit and concentration unit
        /// </summary>
        /// <param name="consumptionIntakeUnit"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="bodyWeightUnit"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public static TargetUnit CreateSingleValueDietaryExposureUnit(
            ConsumptionIntakeUnit consumptionIntakeUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit bodyWeightUnit,
            bool isPerPerson
        ) {
            return CreateDietaryExposureUnit(
                consumptionIntakeUnit.GetConsumptionUnit(),
                concentrationUnit,
                consumptionIntakeUnit.IsPerPerson()
                    ? bodyWeightUnit
                    : consumptionIntakeUnit.GetBodyWeightUnit(),
                isPerPerson
            );
        }

        public static TargetUnit CreateDietaryExposureUnit(
            ConsumptionUnit consumptionUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit bodyWeightUnit,
            bool isPerPerson
        ) {
            var result = new TargetUnit {
                TimeScaleUnit = TimeScaleUnit.PerDay,
                ConcentrationMassUnit = isPerPerson
                    ? ConcentrationMassUnit.PerUnit
                    : ConcentrationMassUnitConverter.FromBodyWeightUnit(bodyWeightUnit)
            };
            if (result.ConcentrationMassUnit == ConcentrationMassUnit.Kilograms) {
                if (consumptionUnit == ConsumptionUnit.g) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.kgPerKg:
                        case ConcentrationUnit.kgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.gPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.mgPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.ugPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.ngPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.pgPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Femtograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.fgPerKgBWPerDay;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit}, concentration unit {concentrationUnit}, and consumption unit {consumptionUnit}");
                    }
                } else if (consumptionUnit == ConsumptionUnit.kg) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.gPerKg:
                            result.SubstanceAmount = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.gPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.mgPerKg:
                            result.SubstanceAmount = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.mgPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.ugPerKg:
                            result.SubstanceAmount = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.ugPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.ngPerKg:
                            result.SubstanceAmount = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.ngPerKgBWPerDay;
                            break;
                        case ConcentrationUnit.pgPerKg:
                            result.SubstanceAmount = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            //unit = IntakeUnit.pgPerKgBWPerDay;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit}, concentration unit {concentrationUnit}, and consumption unit {consumptionUnit}");
                    }
                } else {
                    throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit} and consumption unit {consumptionUnit}");
                }
            } else if (result.ConcentrationMassUnit == ConcentrationMassUnit.Grams) {
                if (consumptionUnit == ConsumptionUnit.g) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.kgPerKg:
                        case ConcentrationUnit.kgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.gPerGBWPerDay;
                            break;
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.mgPerGBWPerDay;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.ugPerGBWPerDay;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.ngPerGBWPerDay;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.pgPerGBWPerDay;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Femtograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.fgPerGBWPerDay;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit}, concentration unit {concentrationUnit}, and consumption unit {consumptionUnit}");
                    }
                } else if (consumptionUnit == ConsumptionUnit.kg) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.gPerGBWPerDay;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.mgPerGBWPerDay;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.ugPerGBWPerDay;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.ngPerGBWPerDay;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            //unit = IntakeUnit.pgPerGBWPerDay;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit}, concentration unit {concentrationUnit}, and consumption unit {consumptionUnit}");
                    }
                } else {
                    throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit} and consumption unit {consumptionUnit}");
                }
            } else if (result.ConcentrationMassUnit == ConcentrationMassUnit.PerUnit) {
                if (consumptionUnit == ConsumptionUnit.g) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.kgPerKg:
                        case ConcentrationUnit.kgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Grams;
                            break;
                        
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Milligrams;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:         
                        case ConcentrationUnit.ugPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Micrograms;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Nanograms;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Picograms;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Femtograms;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit} and consumption unit {consumptionUnit}");
                    }
                } else if (consumptionUnit == ConsumptionUnit.kg) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.kgPerKg:
                        case ConcentrationUnit.kgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Kilograms;
                            break;
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Grams;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Milligrams;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                            result.SubstanceAmount = SubstanceAmountUnit.Micrograms;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Nanograms;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmount = SubstanceAmountUnit.Picograms;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit} and consumption unit {consumptionUnit}");
                    }
                }
            } else {
                throw new Exception($"Failed to create target unit from concentration mass unit {result.ConcentrationMassUnit}");
            }
            return result;
        }
    }
}
