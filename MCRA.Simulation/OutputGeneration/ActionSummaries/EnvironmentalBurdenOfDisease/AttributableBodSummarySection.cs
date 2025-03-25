using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodSummarySection : SummarySection {

        public List<AttributableBodSummaryRecord> Records { get; set; }

        public void Summarize(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases) {
            Records = environmentalBurdenOfDiseases
                .Select(s => new AttributableBodSummaryRecord {
                    BodIndicator = s.BodIndicator.GetShortDisplayName(),
                    ExposureResponseFunctionCode = s.ExposureResponseFunction.Code,
                    ExposureBin = s.ExposureBin.ToString(),
                    Exposure = s.Exposure,
                    Unit = s.Unit,
                    Ratio = s.Ratio,
                    AttributableFraction = s.AttributableFraction,
                    TotalBod = s.TotalBod,
                    AttributableBod = s.AttributableBod
                })
                .ToList();
        }
    }
}
