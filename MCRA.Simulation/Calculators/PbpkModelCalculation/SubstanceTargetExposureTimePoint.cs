namespace MCRA.Simulation.Calculators.PbpkModelCalculation {
    public readonly struct SubstanceTargetExposureTimePoint {
        public SubstanceTargetExposureTimePoint(double time = 0, double exposure = 0) {
            Time = time;
            Exposure = exposure;
        }

        public double Time { get; }

        public double Exposure { get; }

        public override string ToString() => $"{Time:G6}: {Exposure:G6}";
    }
}
