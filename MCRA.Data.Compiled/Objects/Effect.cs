using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Effect : StrongEntity {
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
    }
}
