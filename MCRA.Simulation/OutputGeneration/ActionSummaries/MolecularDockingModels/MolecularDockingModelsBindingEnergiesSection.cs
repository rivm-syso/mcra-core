using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelsBindingEnergiesSection : SummarySection {

        public List<MolecularDockingModelBindingEnergiesRecord> Records { get; set; }

        public void Summarize(ICollection<MolecularDockingModel> molecularDockingModels, HashSet<Compound> compounds) {
            var records = molecularDockingModels
                .Select(r => new MolecularDockingModelBindingEnergiesRecord() {
                    Code = r.Code,
                    Name = r.Name,
                    Threshold = r.Threshold ?? double.NaN,
                    NumberOfReceptors = r.NumberOfReceptors,
                    BindingEnergies = r.BindingEnergies
                        .Where(e => compounds.Contains(e.Key))
                        .Select(e => new MolecularDockingModelCompoundRecord() {
                            SubstanceCode = e.Key.Code,
                            SubstanceName = e.Key.Name,
                            BindingEnergy = e.Value
                        })
                        .ToList(),
                })
                .ToList();
            Records = records;
        }
    }
}
