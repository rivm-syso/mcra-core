using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndoorAirConcentrationsSummarySection : SummarySection {
        public List<IndoorAirConcentrationsDataRecord> Records { get; set; }

        public void Summarize(
            IList<IndoorAirConcentration> indoorAirConcentrations
        ) {
            Records = indoorAirConcentrations
                .Select(c => {
                    return new IndoorAirConcentrationsDataRecord() {
                        idSample = c.idSample,
                        CompoundName = c.Substance.Name,
                        Location = c.Location,
                        Concentration = c.Concentration,
                        Unit = c.AirConcentrationUnit.GetShortDisplayName()
                    };
                })
                .ToList();
        }
    }
}
