using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SoilConcentrationDistributionsSummarySection : SummarySection {
        public List<SoilConcentrationDistributionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<SoilConcentrationDistribution> soilConcentrationDistributions
        ) {
            Records = soilConcentrationDistributions
                .Select(c => {
                    return new SoilConcentrationDistributionsDataRecord() {
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
