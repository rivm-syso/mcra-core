using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;

namespace MCRA.Data.Compiled.Objects {
    public sealed class PbkModelDefinition {
        private string _name;
        public string IdModelDefinition { get; set; }
        public string Name {
            get => string.IsNullOrEmpty(_name) ? IdModelDefinition : _name;
            set => _name = value;
        }
        public string Description { get; set; }
        public string FileName { get; set; }
        public IPbkModelSpecification KineticModelDefinition { get; set; }
    }
}
