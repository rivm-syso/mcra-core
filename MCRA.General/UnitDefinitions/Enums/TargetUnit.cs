using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {

    public class TargetUnit {

        [Flags]
        public enum DisplayOption {
            UnitOnly = 0,
            AppendBiologicalMatrix = 1,
            AppendExpressionType = 2
        }

        public TargetUnit(
            SubstanceAmountUnit substanceAmountUnit,
            ConcentrationMassUnit concentrationMassUnit,
            TimeScaleUnit timeScaleUnit,
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType
        ) {
            SubstanceAmountUnit = substanceAmountUnit;
            ConcentrationMassUnit = concentrationMassUnit;
            TimeScaleUnit = timeScaleUnit;
            BiologicalMatrix = biologicalMatrix;
            ExpressionType = expressionType;
        }

        public TargetUnit(
            SubstanceAmountUnit substanceAmountUnit,
            ConcentrationMassUnit concentrationMassUnit,
            TimeScaleUnit timeScaleUnit
        ) : this(substanceAmountUnit, concentrationMassUnit, timeScaleUnit, BiologicalMatrix.Undefined) {
        }

        public TargetUnit(
            SubstanceAmountUnit substanceAmountUnit,
            ConcentrationMassUnit concentrationMassUnit,
            TimeScaleUnit timeScaleUnit,
            BiologicalMatrix biologicalMatrix
        ) : this(substanceAmountUnit, concentrationMassUnit, timeScaleUnit, biologicalMatrix, ExpressionType.None) {
        }

        public TargetUnit(
           SubstanceAmountUnit substanceAmountUnit,
           ConcentrationMassUnit concentrationMassUnit
        ) : this(substanceAmountUnit, concentrationMassUnit, TimeScaleUnit.Unspecified, BiologicalMatrix.Undefined, ExpressionType.None) {
        }

        public TargetUnit(
           ConcentrationMassUnit concentrationMassUnit,
           TimeScaleUnit timeScaleUnit
        ) : this(SubstanceAmountUnit.Undefined, concentrationMassUnit, timeScaleUnit, BiologicalMatrix.Undefined, ExpressionType.None) {
        }

        public TargetUnit(
            ExposureUnit exposureUnit,
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.Undefined,
            bool? isPerPerson = null
        ) : this(exposureUnit.GetSubstanceAmountUnit(),
            isPerPerson ?? false ? ConcentrationMassUnit.PerUnit : exposureUnit.GetConcentrationMassUnit(),
            exposureUnit.GetTimeScale(), biologicalMatrix, ExpressionType.None) {
        }

        /// <summary>
        /// The unit of the substance amounts.
        /// </summary>
        public SubstanceAmountUnit SubstanceAmountUnit { get; set; }

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
        /// The target biological matrix or compartment. May be internal organs, e.g., liver or, in case of external
        /// target exposures, this could be a person/individual.
        /// </summary>
        public BiologicalMatrix BiologicalMatrix { get; set; }

        /// <summary>
        /// Examples: "lipids", "creatinine"
        /// </summary>
        public ExpressionType ExpressionType { get; }

        public string Code {
            get { return CreateUnitKey(new (BiologicalMatrix.GetDisplayName(), ExpressionType.GetDisplayName())); }
        }

        public static string CreateUnitKey((string BiologicalMatrix, string ExpressionType) key) {
            return $"{key.BiologicalMatrix.ToLower()}:{key.ExpressionType.ToLower()}";
        }

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
        public double GetAlignmentFactor(TargetUnit targetUnit, double molarMass, double unitWeight) {
            var substanceAmountCorrectionFactor = SubstanceAmountUnit.GetMultiplicationFactor(targetUnit.SubstanceAmountUnit, molarMass);
            var concentrationMassCorrectionFactor = ConcentrationMassUnit.GetMultiplicationFactor(targetUnit.ConcentrationMassUnit, unitWeight);
            return substanceAmountCorrectionFactor / concentrationMassCorrectionFactor;
        }

        /// <summary>
        /// Returns the short display name of this unit.
        /// </summary>
        public string GetShortDisplayName(DisplayOption displayOption = DisplayOption.UnitOnly) {
            var substanceAmountString = SubstanceAmountUnit.GetShortDisplayName();
            var perUnitString = string.Empty;
            if (ConcentrationMassUnit != ConcentrationMassUnit.PerUnit) {

                perUnitString = $"/{ConcentrationMassUnit.GetShortDisplayName()}";
                if ((displayOption & DisplayOption.AppendExpressionType) != 0 && ExpressionType != ExpressionType.None) {
                    perUnitString += $" {ExpressionType.GetDisplayName().ToLower()}";
                }
                if ((displayOption & DisplayOption.AppendBiologicalMatrix) != 0 && !BiologicalMatrix.IsUndefined()) {
                    perUnitString += $", ({BiologicalMatrix.GetDisplayName().ToLower()})";
                }
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
        /// Returns the raw display name of this unit.
        /// </summary>
        /// <returns></returns>
        public string GetRawDisplayName() {
            var substanceAmountString = SubstanceAmountUnit.GetShortDisplayName();
            return $"{substanceAmountString}/day";
        }

        /// <summary>
        /// Create a target unit from a dose unit.
        /// </summary>
        public static TargetUnit FromDoseUnit(DoseUnit doseUnit) {
            return new TargetUnit(doseUnit.GetSubstanceAmountUnit(), doseUnit.GetConcentrationMassUnit(), doseUnit.GetTimeScaleUnit(), BiologicalMatrix.Undefined, ExpressionType.None);
        }

        /// <summary>
        /// Create a target unit from a dose unit.
        /// </summary>
        /// <param name="doseUnit"></param>
        /// <param name="compartment"></param>
        /// <returns></returns>
        public static TargetUnit FromDoseUnit(DoseUnit doseUnit, BiologicalMatrix biologicalMatrix) {
            return new TargetUnit(doseUnit.GetSubstanceAmountUnit(), doseUnit.GetConcentrationMassUnit(), doseUnit.GetTimeScaleUnit(), biologicalMatrix, ExpressionType.None);
        }

        /// <summary>
        /// Gets a target unit from a consumption intake unit and concentration unit
        /// </summary>
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
            var result = new TargetUnit(
                isPerPerson ? ConcentrationMassUnit.PerUnit : ConcentrationMassUnitConverter.FromBodyWeightUnit(bodyWeightUnit),
                TimeScaleUnit.PerDay);
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

        public TargetUnit Clone() {
            return new TargetUnit(SubstanceAmountUnit, ConcentrationMassUnit, TimeScaleUnit, BiologicalMatrix, ExpressionType);
        }

        public override string ToString() {
            return GetShortDisplayName(DisplayOption.AppendBiologicalMatrix | DisplayOption.AppendExpressionType);
        }
    }

    public class TargetUnitComparer : IEqualityComparer<TargetUnit> {
        public bool Equals(TargetUnit x, TargetUnit y) {
            return x.SubstanceAmountUnit == y.SubstanceAmountUnit 
                && x.ConcentrationMassUnit == y.ConcentrationMassUnit
                && x.TimeScaleUnit == y.TimeScaleUnit
                && x.BiologicalMatrix == y.BiologicalMatrix
                && x.ExpressionType == y.ExpressionType;
        }

        public int GetHashCode(TargetUnit obj) {
            return HashCode.Combine(obj.SubstanceAmountUnit, obj.ConcentrationMassUnit, obj.TimeScaleUnit, obj.BiologicalMatrix, obj.ExpressionType);
        }
    }
}
