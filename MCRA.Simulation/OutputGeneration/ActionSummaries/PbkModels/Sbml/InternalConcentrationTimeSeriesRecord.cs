using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalConcentrationTimeSeriesRecord {
        public string Compartment { get; set; }
        public TimeUnit TimeScale { get; set; }
        public int TimeFrequency { get; set; }
        public DoseUnit Unit { get; set; }
        public double[] Values { get; set; }
    }
}
