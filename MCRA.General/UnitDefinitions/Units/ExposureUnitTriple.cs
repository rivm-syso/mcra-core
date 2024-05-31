using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {
    public class ExposureUnitTriple {

        public ExposureUnitTriple()
            : this(SubstanceAmountUnit.Undefined, ConcentrationMassUnit.Undefined, TimeScaleUnit.Unspecified) {
        }

        public ExposureUnitTriple(
            SubstanceAmountUnit substanceAmountUnit,
            ConcentrationMassUnit concentrationMassUnit,
            TimeScaleUnit timeScaleUnit = TimeScaleUnit.Unspecified
        ) {
            SubstanceAmountUnit = substanceAmountUnit;
            ConcentrationMassUnit = concentrationMassUnit;
            TimeScaleUnit = timeScaleUnit;
        }

        /// <summary>
        /// The unit of the substance amounts.
        /// </summary>
        public SubstanceAmountUnit SubstanceAmountUnit { get; set; }

        /// <summary>
        /// The object mass unit. Per-unit for absolute amounts (e.g., mg/day),
        /// otherwise a mass unit (e.g., kg for mg/kg BW/day).
        /// </summary>
        public ConcentrationMassUnit ConcentrationMassUnit { get; set; }

        /// <summary>
        /// The time scale. E.g., per day, peak (acute), long term (chronic).
        /// </summary>
        public TimeScaleUnit TimeScaleUnit { get; set; }

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
        public double GetAlignmentFactor(ExposureUnitTriple targetUnit, double molarMass, double unitWeight) {
            var substanceAmountCorrectionFactor = SubstanceAmountUnit.GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, molarMass);
            var concentrationMassCorrectionFactor = ConcentrationMassUnit.GetMultiplicationFactor(targetUnit.ConcentrationMassUnit, unitWeight);
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor;
        }

        /// <summary>
        /// Returns the short display name of this unit.
        /// </summary>
        public string GetShortDisplayName() {
            var substanceAmountString = SubstanceAmountUnit.GetShortDisplayName();
            var perUnitString = string.Empty;
            if (ConcentrationMassUnit != ConcentrationMassUnit.PerUnit) {
                perUnitString = $"/{ConcentrationMassUnit.GetShortDisplayName()}";
            }
            var perTimeUnitString = string.Empty;
            switch (TimeScaleUnit) {
                case TimeScaleUnit.PerDay:
                    perTimeUnitString = "/day";
                    break;
                case TimeScaleUnit.SteadyState:
                case TimeScaleUnit.Peak:
                case TimeScaleUnit.Unspecified:
                default:
                    break;
            }
            return $"{substanceAmountString}{perUnitString}{perTimeUnitString}";
        }

        /// <summary>
        /// Override ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return GetShortDisplayName();
        }

        /// <summary>
        /// Create a target unit from a dose unit.
        /// </summary>
        public static ExposureUnitTriple FromDoseUnit(DoseUnit doseUnit) {
            return new ExposureUnitTriple(
                doseUnit.GetSubstanceAmountUnit(),
                doseUnit.GetConcentrationMassUnit(),
                doseUnit.GetTimeScale()
            );
        }

        /// <summary>
        /// Create a target unit from a dose unit.
        /// </summary>
        public static ExposureUnitTriple FromExposureUnit(ExternalExposureUnit exposureUnit) {
            return new ExposureUnitTriple(
                exposureUnit.GetSubstanceAmountUnit(),
                exposureUnit.GetConcentrationMassUnit(),
                exposureUnit.GetTimeScale()
            );
        }

        public static ExposureUnitTriple FromExposureTarget(ExposureTarget exposureTarget) {
            var defaultUnit = exposureTarget.BiologicalMatrix.GetTargetConcentrationUnit();
            return exposureTarget.ExpressionType switch {
                ExpressionType.None => new ExposureUnitTriple(defaultUnit.GetSubstanceAmountUnit(), defaultUnit.GetConcentrationMassUnit()),
                ExpressionType.Lipids => new ExposureUnitTriple(defaultUnit.GetSubstanceAmountUnit(), ConcentrationMassUnit.Grams),
                ExpressionType.Creatinine => new ExposureUnitTriple(defaultUnit.GetSubstanceAmountUnit(), ConcentrationMassUnit.Grams),
                ExpressionType.SpecificGravity => new ExposureUnitTriple(defaultUnit.GetSubstanceAmountUnit(), defaultUnit.GetConcentrationMassUnit()),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Creates a external (dietary) target exposure unit based on the provided
        /// food consumption unit, concentration unit, bodyweight unit, and specification
        /// of per-person or per-bodyweight.
        /// </summary>
        /// <param name="consumptionUnit"></param>
        /// <param name="concentrationUnit"></param>
        /// <param name="bodyWeightUnit"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ExposureUnitTriple CreateDietaryExposureUnit(
            ConsumptionUnit consumptionUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit bodyWeightUnit,
            bool isPerPerson
        ) {
            var result = new ExposureUnitTriple(
                SubstanceAmountUnit.Undefined,
                isPerPerson
                    ? ConcentrationMassUnit.PerUnit 
                    : ConcentrationMassUnitConverter.FromBodyWeightUnit(bodyWeightUnit),
                TimeScaleUnit.PerDay
            );
            if (result.ConcentrationMassUnit == ConcentrationMassUnit.Kilograms) {
                if (consumptionUnit == ConsumptionUnit.g) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.kgPerKg:
                        case ConcentrationUnit.kgPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                        case ConcentrationUnit.ugPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                        case ConcentrationUnit.ngPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Femtograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit}, concentration unit {concentrationUnit}, and consumption unit {consumptionUnit}");
                    }
                } else if (consumptionUnit == ConsumptionUnit.kg) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.gPerKg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.mgPerKg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.ugPerKg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.ngPerKg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
                            break;
                        case ConcentrationUnit.pgPerKg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Kilograms;
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
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                        case ConcentrationUnit.ugPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                        case ConcentrationUnit.ngPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Femtograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit}, concentration unit {concentrationUnit}, and consumption unit {consumptionUnit}");
                    }
                } else if (consumptionUnit == ConsumptionUnit.kg) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Grams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                        case ConcentrationUnit.ugPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Milligrams;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                        case ConcentrationUnit.ngPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Micrograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Nanograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Picograms;
                            result.ConcentrationMassUnit = ConcentrationMassUnit.Grams;
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
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Grams;
                            break;
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Milligrams;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                        case ConcentrationUnit.ugPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Micrograms;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                        case ConcentrationUnit.ngPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Nanograms;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Picograms;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Femtograms;
                            break;
                        default:
                            throw new Exception($"Failed to create target unit from bodyweight unit {bodyWeightUnit} and consumption unit {consumptionUnit}");
                    }
                } else if (consumptionUnit == ConsumptionUnit.kg) {
                    switch (concentrationUnit) {
                        case ConcentrationUnit.kgPerKg:
                        case ConcentrationUnit.kgPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Kilograms;
                            break;
                        case ConcentrationUnit.gPerKg:
                        case ConcentrationUnit.gPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Grams;
                            break;
                        case ConcentrationUnit.mgPerKg:
                        case ConcentrationUnit.mgPerL:
                        case ConcentrationUnit.mgPerdL:
                        case ConcentrationUnit.ugPermL:
                        case ConcentrationUnit.ugPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Milligrams;
                            break;
                        case ConcentrationUnit.ugPerKg:
                        case ConcentrationUnit.ugPerL:
                        case ConcentrationUnit.ngPermL:
                        case ConcentrationUnit.ngPerg:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Micrograms;
                            break;
                        case ConcentrationUnit.ngPerKg:
                        case ConcentrationUnit.ngPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Nanograms;
                            break;
                        case ConcentrationUnit.pgPerKg:
                        case ConcentrationUnit.pgPerL:
                            result.SubstanceAmountUnit = SubstanceAmountUnit.Picograms;
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
