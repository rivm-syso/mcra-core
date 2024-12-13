namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposuresSection : SummarySection {

        public List<ActionSummaryUnitRecord> CollectUnits(List<ActionSummaryUnitRecord> units, ActionData data) {
            var overrideUnits = new List<string>() { "IntakeUnit" };
            var unitsOut = units.Where(r => !overrideUnits.Contains(r.Type)).ToList() ?? [];
            unitsOut.Add(new ActionSummaryUnitRecord("IntakeUnit", data.ExternalExposureUnit.GetShortDisplayName()));
            return unitsOut;
        }
    }
}
