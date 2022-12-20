namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Holds a unit description/definition for an action summary.
    /// </summary>
    public sealed class ActionSummaryUnitRecord {

        /// <summary>
        /// The type identifier of the unit (e.g., IntakeUnit).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The unit string (e.g., mg/kg BW/day).
        /// </summary>
        public string Unit { get; set; }

        public ActionSummaryUnitRecord() {
        }

        public ActionSummaryUnitRecord(string type, string unit) {
            Type = type;
            Unit = unit;
        }
    }
}
