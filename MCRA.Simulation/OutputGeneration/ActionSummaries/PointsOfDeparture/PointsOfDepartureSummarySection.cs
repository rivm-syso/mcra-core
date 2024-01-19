using MathNet.Numerics.Statistics;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PointsOfDepartureSummarySection : SummarySection {

        public List<PointsOfDepartureSummaryRecord> Records { get; set; }

        public bool AllPodsAtTarget { get; private set; }

        public void Summarize(
            ICollection<Data.Compiled.Objects.PointOfDeparture> PointsOfDeparture,
            bool DoUncertaintyAnalysis
         ) {
            AllPodsAtTarget = PointsOfDeparture.All(p => p.ExposureRoute == ExposureRoute.Undefined);

            Records = PointsOfDeparture.Select(c => {
                var isUncertainty = c.PointOfDepartureUncertains.Any() && DoUncertaintyAnalysis;
                return new PointsOfDepartureSummaryRecord() {
                    Code = c.Code,
                    CompoundCode = c.Compound.Code,
                    CompoundName = c.Compound.Name,
                    EffectCode = c.Effect.Code,
                    EffectName = c.Effect.Name,
                    BiologicalMatrix = c.BiologicalMatrix.GetDisplayName(),
                    ModelEquation = c.DoseResponseModelEquation,
                    ParameterValues = c.DoseResponseModelParameterValues,
                    PointOfDeparture = c.LimitDose,
                    PointOfDepartureType = c.PointOfDepartureType.GetShortDisplayName(),
                    ExposureRoute = c.ExposureRoute.GetShortDisplayName(),
                    System = c.Species,
                    Unit = c.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                    CriticalEffectSize = c.CriticalEffectSize,
                    NumberOfUncertaintySets = isUncertainty ? c.PointOfDepartureUncertains.Count : 0,
                    Median = isUncertainty ? c.PointOfDepartureUncertains.Select(c => c.LimitDose).Percentile(50) : double.NaN,
                    Minimum = isUncertainty ? c.PointOfDepartureUncertains.Min(c => c.LimitDose) : double.NaN,
                    Maximum = isUncertainty ? c.PointOfDepartureUncertains.Max(c => c.LimitDose) : double.NaN,
                };
            }).ToList();
        }
    }
}
