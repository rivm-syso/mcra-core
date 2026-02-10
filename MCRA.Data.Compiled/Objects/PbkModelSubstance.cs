using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;

namespace MCRA.Data.Compiled.Objects {
    [Serializable]
    public sealed class PbkModelSubstance {

        public Compound Substance { get; set; }

        public IPbkModelSubstanceSpecification SubstanceDefinition { get; set; }

    }
}
