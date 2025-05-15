namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public sealed class TargetExposurePerTimeUnit {
        public TargetExposurePerTimeUnit(double time = 0, double exposure = 0, double? bodyWeight = null) {
            Time = time;
            Exposure = exposure;
            BodyWeight = bodyWeight;
        }

        public double Time { get; set; }

        public double Exposure { get; set; }

        public double? BodyWeight { get; set; }

        public override string ToString() => $"{Time}: {Exposure:0.000}";
    }
}
