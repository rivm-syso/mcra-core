﻿
namespace MCRA.Data.Compiled.Wrappers {
    public sealed class UnitVariabilityRecord {
        public double UnitWeight { get; set; }
        public double UnitVariabilityFactor { get; set; }
        public int UnitsInCompositeSample { get; set; }
        public double CoefficientOfVariation { get; set; }
    }
}
