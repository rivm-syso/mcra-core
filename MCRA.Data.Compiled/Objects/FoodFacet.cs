namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodFacet {

        private string _name;

        public FacetDescriptor FacetDescriptor { get; set; }

        public Facet Facet { get; set; }

        public string FullCode => $"{Facet.Code}.{FacetDescriptor.Code}";
        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return FullCode;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }
    }
}
