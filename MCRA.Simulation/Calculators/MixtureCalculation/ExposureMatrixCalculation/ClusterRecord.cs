﻿using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class ClusterRecord {
        public int ClusterId { get; set; }
        public List<Individual> Individuals { get; set; }
        public List<int> Indices {  get ; set; }
    }
}
