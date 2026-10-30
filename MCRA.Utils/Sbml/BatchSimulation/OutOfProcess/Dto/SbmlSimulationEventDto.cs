using MCRA.Utils.Sbml.Simulation;

namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto {
    public sealed class SbmlSimulationEventDto {
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
