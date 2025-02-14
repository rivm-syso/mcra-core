using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureEffectFunctionSummarySection : SummarySection {

        public List<AttributableBodSummaryRecord> Records { get; set; }
        public ExposureEffectFunction ExposureEffectFunction { get; set; }

        public void Summarize(
            List<EnvironmentalBurdenOfDiseaseResultRecord> attributableEbds
        ) {
            var effectGroups = attributableEbds
                .GroupBy(r => r.ExposureEffectFunction);
            if (effectGroups.Count() > 1) {
                throw new NotImplementedException();
            }
            var effectGroup = attributableEbds
                .GroupBy(r => r.ExposureEffectFunction)
                .First();

            ExposureEffectFunction = effectGroup.Key;
            Records = effectGroup
                .Select(s => new AttributableBodSummaryRecord {
                    ExposureBin = s.ExposureBin.ToString(),
                    Exposure = s.Exposure,
                    Ratio = s.Ratio,
                    AttributableFraction = s.AttributableFraction,
                    TotalBod = s.TotalBod,
                    AttributableBod = s.AttributableBod
                })
                .ToList();
        }
    }
}
