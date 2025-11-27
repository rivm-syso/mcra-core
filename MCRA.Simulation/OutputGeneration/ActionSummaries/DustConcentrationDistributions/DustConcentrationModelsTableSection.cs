using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustConcentrationModelsTableSection : DustConcentrationModelSectionBase {
        public void Summarize(
            IDictionary<Compound, ConcentrationModel> concentrationModels
        ) {
            Records = concentrationModels
                .Select(r => createModelSummaryRecord(r.Key, r.Value, false))
                .OrderBy(r => r.SubstanceName)
                .ThenBy(r => r.Id)
                .ToList();
        }
    }
}
