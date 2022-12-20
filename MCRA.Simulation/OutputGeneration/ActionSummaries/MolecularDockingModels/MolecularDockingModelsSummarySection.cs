using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelsSummarySection : SummarySection {

        public List<MolecularDockingModelRecord> Records { get; set; }

        public void Summarize(ICollection<MolecularDockingModel> molecularDockingModels, HashSet<Compound> compounds) {
            var records = molecularDockingModels
                .Select(r => {
                    var bindingEnergies = r.BindingEnergies
                        .Where(e => compounds.Contains(e.Key))
                        .Select(e => e.Value)
                        .ToList();
                    return new MolecularDockingModelRecord() {
                        Code = r.Code,
                        Name = r.Name,
                        Description = r.Description,
                        EffectCode = r.Effect.Code,
                        EffectName = r.Effect.Name,
                        Threshold = r.Threshold ?? double.NaN,
                        NumberOfReceptors = r.NumberOfReceptors,
                        BindingEnergiesCount = bindingEnergies.Count,
                        BindingEnergiesLowerQuartile = (bindingEnergies.Count > 1) ? bindingEnergies.Percentile(25) : double.NaN,
                        BindingEnergiesMedian = (bindingEnergies.Count > 1) ? bindingEnergies.Median() : double.NaN,
                        BindingEnergiesUpperQuartile = (bindingEnergies.Count > 1) ? bindingEnergies.Percentile(75) : double.NaN,
                        BindingEnergiesMinimum = (bindingEnergies.Count > 0) ? bindingEnergies.Min() : double.NaN,
                        BindingEnergiesMaximum = (bindingEnergies.Count > 0) ? bindingEnergies.Max() : double.NaN,
                    };
                })
                .ToList();
            Records = records;
        }
    }
}
