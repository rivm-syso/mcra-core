using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueNonDietaryExposuresSection : ActionSummarySectionBase {
        public List<SingleValueNonDietaryExposureRecord> Records { get; set; }

        public void Summarize(
            IList<ExposureEstimate> exposureEstimates
         ) {
            Records = exposureEstimates
                .OrderBy(s => s.ExposureScenario.Code)
                .Select(s => {
                    var record = new SingleValueNonDietaryExposureRecord {
                        ScenarioName = s.ExposureScenario.Name,
                        SubstanceName = s.Substance.Name,
                        SubstanceCode = s.Substance.Code,
                        ExposureSource = s.ExposureSource,
                        ExposureDeterminantCombinationId = s.ExposureDeterminantCombination?.Code,
                        ExposureDeterminantCombinationName = s.ExposureDeterminantCombination?.Name,
                        EstimateType = s.EstimateType,
                        ExposureValue = s.Value                        
                    };
                    return record;
                })                
                .ToList();            
        }
    }
}
