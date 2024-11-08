namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureDeterminantCombination : StrongEntity {
        public Dictionary<string, ExposureDeterminantValue> Properties { get; set; } = [];
    }
}
