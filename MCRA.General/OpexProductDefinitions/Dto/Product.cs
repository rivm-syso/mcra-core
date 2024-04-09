namespace MCRA.General.OpexProductDefinitions.Dto {
    /// <summary>
    /// OPEX product definition with content that should match exactly with the OPEX product.csv file.
    /// </summary>
    public record Product(
        // NOTE: all properties are in lowerCamelCase, to get the correct casing when used in the OPEX R script
        string name,
        string formulation,
        bool wps,
        string category
    ) {
        public Product() : this ("", "", false, "") { }
    };
}
