using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration.ActionSummaries.ConsumerProductConcentrationDistributions;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductConcentrationModelsGraphSection : ConsumerProductConcentrationModelSectionBase {
        public void Summarize(
            IDictionary<(ConsumerProduct Product, Compound Substance), ConcentrationModel> concentrationModels
        ) {
            Records = concentrationModels
                .Where(c => c.Value.ModelType != ConcentrationModelType.Empirical || c.Value.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = SummarizeBase(r.Key.Product, r.Key.Substance, r.Value);
                    return record;
                })
                .OrderBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ConsumerProductName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
