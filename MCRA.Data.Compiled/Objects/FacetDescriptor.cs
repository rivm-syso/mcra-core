namespace MCRA.Data.Compiled.Objects {
    public sealed class FacetDescriptor : StrongEntity {
        public bool HasName => !string.IsNullOrEmpty(Name);
    }
}
