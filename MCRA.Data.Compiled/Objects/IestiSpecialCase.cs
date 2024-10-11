using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class IestiSpecialCase {
        public Food Food { get; set; }
        public Compound Substance { get; set; }
        public string Reference { get; set; }
        public HarvestApplicationType ApplicationType { get; set; }
    }
}
