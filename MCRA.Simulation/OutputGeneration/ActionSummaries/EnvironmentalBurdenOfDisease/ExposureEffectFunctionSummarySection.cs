using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureEffectFunctionSummarySection : SummarySection {

        public List<AttributableBodSummaryRecord> Records { get; set; }
        public ExposureEffectFunctionSummaryRecord EefRecord { get; set; }
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

            ExposureEffectFunction = effectGroup.Key;

            EefRecord = new ExposureEffectFunctionSummaryRecord {
                SubstanceName = ExposureEffectFunction.Substance.Name,
                SubstanceCode = ExposureEffectFunction.Substance.Code,
                Effect = ExposureEffectFunction.Effect.Name,
                TargetLevel = ExposureEffectFunction.TargetLevel.GetDisplayName(),
                ExposureRoute = ExposureEffectFunction.ExposureRoute != ExposureRoute.Undefined 
                    ? ExposureEffectFunction.ExposureRoute.GetDisplayName()
                    : null,
                BiologicalMatrix = ExposureEffectFunction.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? ExposureEffectFunction.BiologicalMatrix.GetDisplayName()
                    : null,
                DoseUnit = ExposureEffectFunction.DoseUnit.GetShortDisplayName(),
                ExpressionType = ExposureEffectFunction.ExpressionType != ExpressionType.None
                    ? ExposureEffectFunction.ExpressionType.GetDisplayName()
                    : null,
                EffectMetric = ExposureEffectFunction.EffectMetric.GetDisplayName(),
                ExposureResponesType = ExposureEffectFunction.ExposureResponseType.GetDisplayName(),
                ExposureResponseSpecification = ExposureEffectFunction.ExposureResponseSpecification.ExpressionString,
                Baseline = ExposureEffectFunction.Baseline
            };
        }
    }
}
