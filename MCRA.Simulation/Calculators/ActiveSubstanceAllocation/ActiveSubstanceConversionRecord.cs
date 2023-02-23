using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {
    public sealed class ActiveSubstanceConversionRecord {
        public SampleCompound MeasuredSubstanceSampleCompound { get; set; }
        public ICollection<SampleCompound> ActiveSubstanceSampleCompounds { get; set; }
        public bool Authorised { get; set; }
    }
}
