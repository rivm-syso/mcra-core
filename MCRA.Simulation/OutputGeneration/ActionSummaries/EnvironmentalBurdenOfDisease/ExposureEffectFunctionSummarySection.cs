using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureEffectFunctionSummarySection : SummarySection {

        public List<AttributableEbdSummaryRecord> Records { get; set; }
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
                .Select(s => new AttributableEbdSummaryRecord {
                    PercentileInterval = s.PercentileInterval.ToString(),
                    ExposureLevel = s.ExposureLevel,
                    PercentileSpecificOr = s.PercentileSpecificOr,
                    PercentileSpecificAf = s.PercentileSpecificAf,
                    AbsoluteBod = s.AbsoluteBod,
                    AttributableEbd = s.AttributableEbd
                })
                .ToList();
        }
    }
}
