using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PopulationCharacteristicsSummarySection : SummarySection {
        public List<PopulationCharacteristicsSummaryRecord> Records { get; set; }

        public void Summarize(
            ICollection<PopulationCharacteristic> characteristics
        ) {
            Records = characteristics
                .Select(r => new PopulationCharacteristicsSummaryRecord() {
                    PopulationCode = r.idPopulation,
                    Characteristic = r.Characteristic.ToString(),
                    Unit = r.Unit,
                    DistributionType = r.DistributionType.ToString(),
                    Value = r.Value,
                    CV = r.CvVariability
                })
                .ToList();
        }
    }
}
