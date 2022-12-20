namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseRecord {
        public double Dose { get; set; }
        public double Response { get; set; }
        public int N { get; set; } = 0;
        public double SD { get; set; } = double.NaN;
    }
}
