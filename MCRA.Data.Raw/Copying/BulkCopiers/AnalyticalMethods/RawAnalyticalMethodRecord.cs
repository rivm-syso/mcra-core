using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Copying.BulkCopiers.HumanMonitoring {
    public sealed class RawAnalyticalMethodRecord {
        public string idAnalyticalMethod { get; set; }
        public string Description { get; set; }
        public Dictionary<string, RawAnalyticalMethodCompound> AnalyticalMethodCompounds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
