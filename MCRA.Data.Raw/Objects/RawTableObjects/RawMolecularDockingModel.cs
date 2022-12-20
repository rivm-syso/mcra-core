using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.MolecularDockingModels)]
    public sealed class RawMolecularDockingModel {
        public string id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idEffect { get; set; }
        public double Threshold { get; set; }
        public int? NumberOfReceptors { get; set; }
        public string Reference { get; set; }
    }
}
