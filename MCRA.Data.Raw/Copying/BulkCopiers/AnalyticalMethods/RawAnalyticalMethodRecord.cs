using MCRA.Data.Raw.Objects.RawObjects;
using System;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers.HumanMonitoring {
    public sealed class RawAnalyticalMethodRecord {
        public string idAnalyticalMethod { get; set; }
        public string Description { get; set; }
        public Dictionary<string, RawAnalyticalMethodCompound> AnalyticalMethodCompounds { get; set; } = new Dictionary<string, RawAnalyticalMethodCompound>(StringComparer.OrdinalIgnoreCase);
    }
}
