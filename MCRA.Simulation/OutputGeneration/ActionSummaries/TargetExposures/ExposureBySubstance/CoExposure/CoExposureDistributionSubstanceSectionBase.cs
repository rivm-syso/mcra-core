using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Collections;

namespace MCRA.Simulation.OutputGeneration {
    public class CoExposureDistributionSubstanceSectionBase : CoExposureSectionBase {

        protected void Summarize(
            ICollection<AggregateIndividualExposure> targetExposures,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var substancesArray = substances.OrderBy(g => g.Code, StringComparer.OrdinalIgnoreCase).ToArray();

            var coExposure = targetExposures
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(c => {
                    if (c.IsPositiveTargetExposure(targetUnit.Target)) {
                        var pattern = new BitPattern32(substancesArray.Length);
                        for (int i = 0; i < substancesArray.Length; i++) {
                            if (c.GetSubstanceExposure(targetUnit.Target, substancesArray[i]) > 0) {
                                pattern.Set(i);
                            }
                        }
                        return pattern;
                    } else {
                        return new BitPattern32(substancesArray.Length);
                    }
                })
                .ToList();
            var rawGroupedExposurePatterns = CalculateGroupedExposuresPatterns(coExposure, substances);
            var groupedExposurePatterns = GetGroupedExposurePatterns(rawGroupedExposurePatterns);
            AggregatedExposureRecords = GetAggregateRecords(targetExposures.Count, rawGroupedExposurePatterns)
                .OrderBy(r => r.NumberOfSubstances).ToList();
            UpperFullExposureRecords = groupedExposurePatterns;
            LowerFullExposureRecords = groupedExposurePatterns
                .OrderBy(c => c.NumberOfSubstances)
                .ThenByDescending(c => c.Frequency)
                .ThenBy(c => c.Substances)
                .ToList();
            UpperFullExposureRecordsExtended = GetExposurePatternFrequencies(
                rawGroupedExposurePatterns,
                substances
            );
        }
    }
}
