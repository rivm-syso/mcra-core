namespace MCRA.Data.Compiled.Objects {
    public sealed class FacetDescriptor : IStrongEntity {
        private string _name;

        public string Code { get; set; }

        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return Code;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }

        public string Description { get; set; }

        public bool HasName() {
            return !string.IsNullOrEmpty(_name);
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
