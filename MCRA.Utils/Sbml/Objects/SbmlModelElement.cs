namespace MCRA.Utils.Sbml.Objects {
    public class SbmlModelElement {
        public string Id { get; set; }
        public string MetaId { get; set; }
        public string Name { get; set; }
        public List<string> BqbIsResources { get; set; }
        public List<string> BqmIsResources { get; set; }
    }
}
