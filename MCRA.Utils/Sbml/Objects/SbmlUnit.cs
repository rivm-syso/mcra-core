namespace MCRA.Utils.Sbml.Objects {
    public class SbmlUnit {
        public SbmlUnitKind Kind { get; set; }
        public decimal Exponent { get; set; }
        public decimal Scale { get; set; }
        public decimal Multiplier { get; set; }
    }
}
