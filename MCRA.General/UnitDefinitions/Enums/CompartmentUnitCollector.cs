namespace MCRA.General.UnitDefinitions.Enums {

    /// <summary>
    /// Use this class to collect units based on different compartments.
    /// </summary>
    public class CompartmentUnitCollector {
        private TimeScaleUnit _timeScaleUnit;

        public CompartmentUnitCollector(TimeScaleUnit timeScaleUnit) {
            _timeScaleUnit = timeScaleUnit;
        }

        public List<TargetUnit> CollectedTargetUnits { get; private set; } = new();

        public void EnsureUnit(SubstanceAmountUnit substanceAmountUnit, ConcentrationMassUnit concentrationMassUnit, BiologicalMatrix biologicalMatrix) {
            if (!CollectedTargetUnits.Exists(u => u.BiologicalMatrix == biologicalMatrix)) {
                CollectedTargetUnits.Add(new TargetUnit(substanceAmountUnit, concentrationMassUnit, _timeScaleUnit, biologicalMatrix, ExpressionType.None));
            }
        }

        public void EnsureUnit(SubstanceAmountUnit substanceAmountUnit, ConcentrationMassUnit concentrationMassUnit, BiologicalMatrix biologicalMatrix, ExpressionType expressionType) {
            if (!CollectedTargetUnits.Exists(u => u.ExpressionType == expressionType)) {
                CollectedTargetUnits.Add(new TargetUnit(substanceAmountUnit, concentrationMassUnit, _timeScaleUnit, biologicalMatrix, expressionType));
            }
        }
    }
}
