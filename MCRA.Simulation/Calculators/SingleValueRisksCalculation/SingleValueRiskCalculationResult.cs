﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public class SingleValueRiskCalculationResult {
        public IExposureSource Source { get; set; }
        public Compound Substance { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
        public double HazardCharacterisation { get; set; }
        public double ExposureHazardRatio { get; set; }
        public double HazardExposureRatio { get; set; }
        public PotencyOrigin Origin { get; set; }
    }
}
