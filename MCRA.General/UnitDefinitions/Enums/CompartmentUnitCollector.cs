namespace MCRA.General.UnitDefinitions.Enums {

    /// <summary>
    /// Use this class to collect units based on different compartments.
    /// </summary>
    public class CompartmentUnitCollector {
        private TimeScaleUnit _timeScaleUnit;

        public CompartmentUnitCollector(TimeScaleUnit timeScaleUnit) {
            _timeScaleUnit = timeScaleUnit;
        }

        public List<TargetUnit> CollectedTargetUnits { get; private set; } = new List<TargetUnit>();

        public void EnsureUnit(SubstanceAmountUnit substanceAmountUnit, ConcentrationMassUnit concentrationMassUnit, BiologicalMatrix biologicalMatrix) {
            if (!CollectedTargetUnits.Exists(u => u.BiologicalMatrix == biologicalMatrix)) {
                CollectedTargetUnits.Add(new TargetUnit(substanceAmountUnit, concentrationMassUnit, _timeScaleUnit, biologicalMatrix, string.Empty));
            }
        }

        public void EnsureUnit(SubstanceAmountUnit substanceAmountUnit, ConcentrationMassUnit concentrationMassUnit, BiologicalMatrix biologicalMatrix, string expressionType) {
            if (!CollectedTargetUnits.Exists(u => string.Equals(u.ExpressionType, expressionType, StringComparison.InvariantCultureIgnoreCase))) {
                CollectedTargetUnits.Add(new TargetUnit(substanceAmountUnit, concentrationMassUnit, _timeScaleUnit, biologicalMatrix, expressionType));
            }
        }
    }
}
