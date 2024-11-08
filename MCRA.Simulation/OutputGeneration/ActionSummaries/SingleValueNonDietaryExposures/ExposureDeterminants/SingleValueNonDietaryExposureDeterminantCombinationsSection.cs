using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueNonDietaryExposureDeterminantCombinationsSection : ActionSummarySectionBase {

        public List<SingleValueNonDietaryExposureDeterminantValueRecord> DeterminantCombinationValueRecords { get; set; }

        public void Summarize(
            IDictionary<string, ExposureDeterminantCombination> exposureDeterminantCombinations
         ) {
            DeterminantCombinationValueRecords = exposureDeterminantCombinations
                .SelectMany(e => e.Value.Properties.Select(p => {
                    var record = new SingleValueNonDietaryExposureDeterminantValueRecord {
                        ExposureDeterminantCombinationId = e.Value.Code,
                        ExposureDeterminant = p.Value.Property.Code,
                        ExposureDeterminantDescription = p.Value.Property.Description,
                        Value = p.Value.DisplayValue
                    };
                    return record;
                }))
                .OrderBy(s => s.ExposureDeterminantCombinationId)
                .ToList();
        }
    }
}
