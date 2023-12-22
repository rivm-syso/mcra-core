using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationLimitsDataSection : ActionSummarySectionBase {
        public List<ConcentrationLimitsDataRecord> Records { get; set; }

        public void Summarize(
            ICollection<ConcentrationLimit> limits,
            ConcentrationUnit concentrationUnit
        ) {
            var allRecords = limits
                .Select(r => {
                    var unitCorrection = r.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                    return new ConcentrationLimitsDataRecord() {
                        CompoundName = r.Compound.Name,
                        CompoundCode = r.Compound.Code,
                        FoodCode = r.Food.Code,
                        FoodName = r.Food.Name,
                        StartDate = r.StartDate?.ToString("MMM yyyy"),
                        EndDate = r.EndDate?.ToString("MMM yyyy"),
                        MaximumConcentrationLimit = unitCorrection * r.Limit,
                    };
                })
               .ToList();
            Records = allRecords
                .Where(c => c.MaximumConcentrationLimit != null)
                .OrderBy(c => c.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
