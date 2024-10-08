using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustConcentrationDistributionsSummarySection : SummarySection {
        public List<DustConcentrationDistributionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<DustConcentrationDistribution> dustConcentrationDistributions
        ) {
            Records = dustConcentrationDistributions
                .Select(c => {
                    return new DustConcentrationDistributionsDataRecord() {
                        idSample = c.idSample,
                        CompoundName = c.Substance.Name,
                        Concentration = c.Concentration,
                        Unit = c.ConcentrationUnit.GetShortDisplayName()
                    };
                })
                .ToList();
        }
    }
}
