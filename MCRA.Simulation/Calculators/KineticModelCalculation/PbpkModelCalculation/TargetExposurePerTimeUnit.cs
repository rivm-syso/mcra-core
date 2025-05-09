﻿namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public sealed class TargetExposurePerTimeUnit {
        public TargetExposurePerTimeUnit(double time = 0, double exposure = 0) {
            Time = time;
            Exposure = exposure;
        }

        public double Time { get; set; }

        public double Exposure { get; set; }

        public override string ToString() => $"{Time}: {Exposure:0.000}";
    }
}
