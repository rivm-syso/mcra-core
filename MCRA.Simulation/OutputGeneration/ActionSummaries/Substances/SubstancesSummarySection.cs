using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubstancesSummarySection : SummarySection {
        public List<SubstanceSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<Compound> substances, Compound reference) {
            Records = substances.Select(r => new SubstanceSummaryRecord() {
                CompoundCode = r.Code,
                CompoundName = r.Name,
                MolecularWeight = r.MolecularMass,
                CramerClass = r.CramerClass?.ToString(),
                IsReference = r == reference
            }).ToList();
        }
    }
}
