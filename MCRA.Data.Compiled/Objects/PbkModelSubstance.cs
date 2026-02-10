using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    [Serializable]
    public sealed class PbkModelSubstance {

        public Compound Substance { get; set; }

        public PbkModelSubstanceSpecification SubstanceDefinition { get; set; }

    }
}
