using MCRA.General;

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
        public string FilePath { get; set; }
        public KineticModelDefinition KineticModelDefinition { get; set; }
    }
}
