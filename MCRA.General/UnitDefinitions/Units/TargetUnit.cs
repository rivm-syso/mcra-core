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
            ExposureTarget target,
            ExposureUnitTriple exposureUnit
        ) {
            Target = target;
            ExposureUnit = exposureUnit;
        }

        public TargetUnit(
            ExposureTarget target,
            SubstanceAmountUnit substanceAmountUnit,
            ConcentrationMassUnit concentrationMassUnit,
            TimeScaleUnit timeScaleUnit = TimeScaleUnit.Unspecified
        ) {
            ExposureUnit = new ExposureUnitTriple(
                substanceAmountUnit,
                concentrationMassUnit,
                timeScaleUnit
            );
            Target = target;
        }

        /// <summary>
        /// The target to which this unit belongs.
        /// </summary>
        public ExposureTarget Target { get; set; }

        /// <summary>
        /// THe exposure unit.
        /// </summary>
        public ExposureUnitTriple ExposureUnit { get; set; }

        /// <summary>
        /// The unit of the substance amounts.
        /// </summary>
        public SubstanceAmountUnit SubstanceAmountUnit {
            get { 
                return ExposureUnit.SubstanceAmountUnit;
            }
        }

        /// <summary>
        /// The object mass unit. per-unit for absolute amounts (e.g., mg/day), otherwise
        /// a mass unit (e.g., kg for mg/kg BW/day).
        /// </summary>
        public ConcentrationMassUnit ConcentrationMassUnit {
            get {
                return ExposureUnit.ConcentrationMassUnit;
            }
        }

        /// <summary>
        /// The time scale. E.g., per day, peak (acute), long term (chronic).
        /// </summary>
        public TimeScaleUnit TimeScaleUnit {
            get {
                return ExposureUnit.TimeScaleUnit;
            }
        }

        /// <summary>
        /// The target level/type of the target (i.e., internal or external).
        /// </summary>
        public TargetLevelType TargetLevelType {
            get {
                return Target.TargetLevelType;
            }
        }

        /// <summary>
        /// Exposure route (for external exposures).
        /// </summary>
        public ExposureRouteType ExposureRoute {
            get {
                return Target.ExposureRoute;
            }
        }

        /// <summary>
        /// The target biological matrix or compartment. May be 
        /// internal organs, e.g., liver or, in case of external
        /// target exposures, this could be a person/individual.
        /// </summary>
        public BiologicalMatrix BiologicalMatrix {
            get {
                return Target.BiologicalMatrix;
            }
        }

        /// <summary>
        /// Examples: "lipids", "creatinine"
        /// </summary>
        public ExpressionType ExpressionType {
            get {
                return Target.ExpressionType;
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
            return ExposureUnit.GetAlignmentFactor(targetUnit.ExposureUnit, molarMass, unitWeight);
        }

        /// <summary>
        /// Returns the short display name of this unit.
        /// </summary>
        public string GetShortDisplayName(DisplayOption displayOption = DisplayOption.UnitOnly) {
            var substanceAmountString = SubstanceAmountUnit.GetShortDisplayName();

            var perUnitString = string.Empty;
            if (ConcentrationMassUnit != ConcentrationMassUnit.PerUnit) {
                perUnitString = $"/{ConcentrationMassUnit.GetShortDisplayName()}";
                if (Target.TargetLevelType == TargetLevelType.Internal) {
                    // Internal target exposure unit
                    if ((displayOption & DisplayOption.AppendExpressionType) != 0 && ExpressionType != ExpressionType.None) {
                        perUnitString += $" {ExpressionType.GetDisplayName().ToLower()}";
                    }
                    if ((displayOption & DisplayOption.AppendBiologicalMatrix) != 0 && !BiologicalMatrix.IsUndefined()) {
                        perUnitString += $" {BiologicalMatrix.GetShortDisplayName()}";
                    }
                } else {
                    // External target exposure unit
                    perUnitString += " bw";
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
        /// Create a target unit from a dose unit.
        /// </summary>
        public static TargetUnit FromExternalExposureUnit(
            ExternalExposureUnit exposureUnit,
            ExposureRouteType route = ExposureRouteType.Dietary
        ) {
            return new TargetUnit(
                new ExposureTarget(route),
                ExposureUnitTriple.FromExposureUnit(exposureUnit)
            );
        }

        /// <summary>
        /// Create a target unit from a dose unit.
        /// </summary>
        public static TargetUnit FromInternalDoseUnit(
            DoseUnit doseUnit,
            BiologicalMatrix biologicalMatrix = BiologicalMatrix.WholeBody,
            ExpressionType expressionType = ExpressionType.None
        ) {
            return new TargetUnit(
                new ExposureTarget(biologicalMatrix, expressionType),
                doseUnit.GetSubstanceAmountUnit(),
                doseUnit.GetConcentrationMassUnit(),
                doseUnit.GetTimeScale()
            );
        }

        /// <summary>
        /// Creates a target unit from a consumption intake unit and concentration unit.
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
        public static TargetUnit CreateDietaryExposureUnit(
            ConsumptionUnit consumptionUnit,
            ConcentrationUnit concentrationUnit,
            BodyWeightUnit bodyWeightUnit,
            bool isPerPerson
        ) {
            var result = new TargetUnit(
                ExposureTarget.DietaryExposureTarget,
                ExposureUnitTriple.CreateDietaryExposureUnit(
                    consumptionUnit,
                    concentrationUnit,
                    bodyWeightUnit,
                    isPerPerson
                )
            );
            return result;
        }

        public override string ToString() {
            return GetShortDisplayName(DisplayOption.AppendBiologicalMatrix | DisplayOption.AppendExpressionType);
        }
    }
}
