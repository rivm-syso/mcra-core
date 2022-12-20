using System;

namespace MCRA.Data.Compiled.Objects {
    public sealed class UnitVariabilityFactor {
        public Compound Compound { get; set; }
        public Food Food { get; set; }
        public ProcessingType ProcessingType { get; set; }

        public double? Factor { get; set; }
        public double UnitsInCompositeSample { get; set; }
        public double? Coefficient { get; set; }

        public UnitVariabilityFactor() {
        }

        public UnitVariabilityFactor(double defaultFactorLow, double defaultFactorMid, double unitWeight) {
            Factor = Convert.ToInt32(unitWeight <= 25) * defaultFactorLow + Convert.ToInt32(unitWeight > 25) * defaultFactorMid;
        }
    }
}
