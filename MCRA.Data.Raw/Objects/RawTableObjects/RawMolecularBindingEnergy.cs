using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.MolecularBindingEnergies)]
    public sealed class RawMolecularBindingEnergy {
        public string idMolecularDockingModel { get; set; }
        public string idCompound { get; set; }
        public double BindingEnergy { get; set; }
    }
}
