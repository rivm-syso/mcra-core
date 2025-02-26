using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EnvironmentalBurdenOfDiseaseSummarySection : ActionSummarySectionBase {
        public EnvironmentalBurdenOfDiseaseSummaryRecord Record { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {
            Record = new EnvironmentalBurdenOfDiseaseSummaryRecord() {
                TotalAttributableBod = environmentalBurdenOfDiseases.Sum(r => r.AttributableBod)
            };

        }
    }
}


