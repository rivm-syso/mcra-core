using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OutdoorAirConcentrationsSummarySection : SummarySection {
        public List<OutdoorAirConcentrationsDataRecord> Records { get; set; }

        public void Summarize(
            IList<OutdoorAirConcentration> outdoorAirConcentrations
        ) {
            Records = outdoorAirConcentrations
                .Select(c => {
                    return new OutdoorAirConcentrationsDataRecord() {
                        idSample = c.idSample,
                        CompoundName = c.Substance.Name,
                        Location = c.Location,
                        Concentration = c.Concentration,
                        Unit = c.Unit.GetShortDisplayName()
                    };
                })
                .ToList();
        }
    }
}
