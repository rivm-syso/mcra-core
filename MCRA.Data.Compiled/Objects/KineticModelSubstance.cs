using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    [Serializable]
    public sealed class KineticModelSubstance {

        public Compound Substance { get; set; }

        public KineticModelSubstanceDefinition SubstanceDefinition { get; set; }

    }
}
