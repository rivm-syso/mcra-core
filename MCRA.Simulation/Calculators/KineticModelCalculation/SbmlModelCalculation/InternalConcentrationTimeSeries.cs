using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation {
    public class InternalConcentrationTimeSeries {
        public string Id { get; set; }
        public DoseUnit Unit { get; set; }
        public TimeUnit TimeScale { get; set; }
        public int TimeFrequency { get; set; }
        public double[] Values { get; set; }
    }
}
