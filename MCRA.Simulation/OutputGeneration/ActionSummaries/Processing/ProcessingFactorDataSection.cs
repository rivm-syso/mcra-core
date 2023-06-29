using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ProcessingFactorDataSection : SummarySection {

        public int DuplicateEntryCount { get; set; }

        public int InconsistendEntryCount { get; set; }

        public List<ProcessingFactorDataRecord> Records { get; set; }

        public void Summarize(ICollection<ProcessingFactor> processingFactors) {
            var duplicateEntries = processingFactors
                .GroupBy(r => (r.FoodUnprocessed, r.Compound, r.ProcessingType))
                .Where(r => r.Count() > 1);
            DuplicateEntryCount = duplicateEntries.Count();
            InconsistendEntryCount = duplicateEntries.Count(g => g.GroupBy(r => (r.Nominal, r.NominalUncertaintyUpper, r.Upper, r.UpperUncertaintyUpper)).Count() > 1);
            Records = processingFactors
                .Select(r => new ProcessingFactorDataRecord() {
                    FoodName = r.FoodUnprocessed.Name,
                    FoodCode = r.FoodUnprocessed.Code,
                    SubstanceName = r.Compound?.Name,
                    SubstanceCode = r.Compound?.Code,
                    ProcessingTypeName = r.ProcessingType.Name,
                    ProcessingTypeCode = r.ProcessingType.Code,
                    Nominal = r.Nominal,
                    Upper = r.Upper ?? double.NaN,
                    BulkingBlending = r.ProcessingType.IsBulkingBlending ? "yes" : "no",
                    Distribution = r.ProcessingType.DistributionType.ToString()
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ProcessingTypeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ProcessingTypeCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
