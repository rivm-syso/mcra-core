namespace MCRA.Simulation.OutputGeneration {
    public class NonDietaryExposureSection : SummarySection {

        public List<ActionSummaryUnitRecord> CollectUnits(List<ActionSummaryUnitRecord> units, ActionData data) {
            var overrideUnits = new List<string>() { "IntakeUnit" };
            var unitsOut = units.Where(r => !overrideUnits.Contains(r.Type)).ToList() ?? new List<ActionSummaryUnitRecord>();
            unitsOut.Add(new ActionSummaryUnitRecord("IntakeUnit", data.ExternalExposureUnit.GetShortDisplayName(false)));
            return unitsOut;
        }
    }
}
