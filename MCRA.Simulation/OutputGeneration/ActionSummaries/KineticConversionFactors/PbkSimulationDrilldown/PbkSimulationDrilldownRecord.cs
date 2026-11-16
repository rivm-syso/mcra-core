using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    public class PbkSimulationDrilldownRecord {

        public string SimulationId { get; set; }

        public double Dose { get; set; }

        public ExposureRoute DoseRoute { get; set; }

        public string SubstanceCode { get; set; }

        public string SubstanceName { get; set; }

        public string EndpointCode { get; set; }

        public string EndpointName { get; set; }

        public string EndpointUnit { get; set; }

        public List<(double Time, double Value)> OutputTimeSeries { get; set; }

    }
}
