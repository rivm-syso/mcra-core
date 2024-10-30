using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableEbdSummarySection : SummarySection {

        public List<AttributableEbdSummaryRecord> Records { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> attributableEbds) {
            Records = attributableEbds
                .Select(s => {
                    var record = new AttributableEbdSummaryRecord {
                        PercentileInterval = s.PercentileInterval.ToString(),
                        Unit = s.Unit,
                        ExposureLevel = s.ExposureLevel,
                        PercentileSpecificOr = s.PercentileSpecificOr,
                        PercentileSpecificAf = s.PercentileSpecificAf,
                        AbsoluteBod = s.AbsoluteBod,
                        AttributableEbd = s.AttributableEbd
                    };
                    return record;
                })
                .ToList();
        }
    }
}
