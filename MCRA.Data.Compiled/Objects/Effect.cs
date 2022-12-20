using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Effect : IStrongEntity {
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

        public string BiologicalOrganisationString { get; set; }
        public string KeyEventProcess { get; set; }
        public string KeyEventObject { get; set; }
        public string KeyEventAction { get; set; }
        public string KeyEventCell { get; set; }
        public string KeyEventOrgan { get; set; }
        public string AOPWikiIds { get; set; }
        public string Reference { get; set; }

        public bool? IsGenotoxic { get; set; }
        public bool? IsAChEInhibitor { get; set; }
        public bool? IsNonGenotoxicCarcinogenic { get; set; }

        public BiologicalOrganisationType BiologicalOrganisationType {
            get {
                return BiologicalOrganisationTypeConverter.FromString(BiologicalOrganisationString);
            }
            set {
                BiologicalOrganisationString = value.ToString();
            }
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
