
namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureDeterminantCombination : IStrongEntity {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, ExposureDeterminantValue> Properties { get; set; } = new();
    }
}
