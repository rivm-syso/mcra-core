namespace MCRA.Utils.Sbml.Simulation {

    public enum SbmlSimulationEventType {
        Single,
        Repeated
    }

    public sealed class SbmlSimulationEvent {
        public string Id { get; set; }
        public SbmlSimulationEventType Kind { get; set; }
        public string Target { get; set; }
        public double Time { get; set; }
        public double Interval { get; set; }
        public double? TimeEnd { get; set; }
        public double Value { get; set; }
        public string Adjustment { get; set; }
    }
}
