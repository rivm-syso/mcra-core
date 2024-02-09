using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueNonDietaryExposuresSection : ActionSummarySectionBase {

        public List<SingleValueNonDietaryExposureRecord> Records { get; set; }

        public List<SingleValueNonDietaryExposureDeterminantValueRecord> DeterminantValueRecords { get; set; }

        public void Summarize(
            IList<ExposureEstimate> exposureEstimates,
            IDictionary<string, ExposureDeterminantCombination> exposureDeterminantCombinations
         ) {
            Records = exposureEstimates
                .Select(s => {
                    var record = new SingleValueNonDietaryExposureRecord {
                        ScenarioName = s.ExposureScenario.Name,
                        SubstanceName = s.Substance.Name,
                        SubstanceCode = s.Substance.Code,
                        ExposureSource = s.ExposureSource,
                        ExposureDeterminantCombinationId = s.ExposureDeterminantCombination?.Code,
                        EstimateType = s.EstimateType,
                        ExposureValue = s.Value                        
                    };
                    return record;
                })
                .OrderBy(s => s.ScenarioName)
                .ToList();

            var exposureDeterminantCombinationsUsed = exposureEstimates
                .Where(s => s.ExposureDeterminantCombination != null)
                .Select(s => s.ExposureDeterminantCombination)
                .Distinct()
                .ToList();
            DeterminantValueRecords = exposureDeterminantCombinations
                .Where(e => exposureDeterminantCombinationsUsed.Any(d => d.Code == e.Key))
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
