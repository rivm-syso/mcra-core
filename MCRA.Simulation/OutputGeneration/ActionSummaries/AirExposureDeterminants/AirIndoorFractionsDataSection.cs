using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AirIndoorFractionsDataSection : SummarySection {
        public List<AirIndoorFractionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<AirIndoorFraction> airIndoorFractions
        ) {
            Records = airIndoorFractions
                .Select(c => {
                    return new AirIndoorFractionsDataRecord() {
                        idSubgroup = c.idSubgroup,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Fraction = c.Fraction,
                    };
                })
                .ToList();
        }
    }
}