using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueNonDietaryExposureScenariosSection : ActionSummarySectionBase {
        public List<SingleValueNonDietaryExposureScenarioRecord> Records { get; set; }

        public void Summarize(
            IDictionary<string, ExposureScenario> exposureScenarios
         ) {
            Records = exposureScenarios
                .Select(s => {
                    return new SingleValueNonDietaryExposureScenarioRecord {
                        ScenarioName = s.Value.Name,
                        Description = s.Value.Description,
                        ExposureLevel = s.Value.ExposureLevel.GetDisplayName(),
                        ExposureRoutes = s.Value.ExposureRoutes,
                        ExposureType = s.Value.ExposureType.GetDisplayName(),
                        ExposureUnit = s.Value.ExposureUnit.GetShortDisplayName(),
                        Population = s.Value.Population.Name
                    };
                })
                .ToList();
        }
    }
}
