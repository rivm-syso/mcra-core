using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodSummarySection : SummarySection {

        public List<AttributableBodSummaryRecord> Records { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases) {
            Records = environmentalBurdenOfDiseases
                .Select(s => {
                    var record = new AttributableBodSummaryRecord {
                        ExposureBin = s.ExposureBin.ToString(),
                        Exposure = s.Exposure,
                        Unit = s.Unit,
                        Ratio = s.Ratio,
                        AttributableFraction = s.AttributableFraction,
                        TotalBod = s.TotalBod,
                        AttributableBod = s.AttributableBod
                    };
                    return record;
                })
                .ToList();
        }
    }
}
