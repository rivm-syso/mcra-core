using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustConcentrationModelsGraphSection : DustConcentrationModelSectionBase {
        public void Summarize(
            IDictionary<Compound, ConcentrationModel> concentrationModels
        ) {
            Records = concentrationModels
                .Where(c => c.Value.ModelType != ConcentrationModelType.Empirical || c.Value.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = createModelSummaryRecord(r.Key, r.Value);
                    return record;
                })
                .OrderBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
