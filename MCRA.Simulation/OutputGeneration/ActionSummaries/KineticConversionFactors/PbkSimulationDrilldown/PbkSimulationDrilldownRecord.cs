namespace MCRA.Simulation.OutputGeneration {

    public class PbkSimulationDrilldownRecord {

        public string SimulationId { get; set; }

        public string EndpointCode { get; set; }

        public string EndpointName { get; set; }

        public string Unit { get; set; }

        public List<(double Time, double Value)> OutputTimeSeries { get; set; }

    }
}
