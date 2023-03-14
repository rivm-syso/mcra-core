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

        public void EnsureUnit(SubstanceAmountUnit substanceAmountUnit, ConcentrationMassUnit concentrationMassUnit, string compartment) {
            if (!CollectedTargetUnits.Exists(u => u.Compartment == compartment)) {
                CollectedTargetUnits.Add(new TargetUnit(substanceAmountUnit, concentrationMassUnit, compartment, _timeScaleUnit));
            }
        }
    }
}
