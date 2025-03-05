using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class CoExposureDistributionSectionBase : CoExposureSectionBase {

        protected void SummarizeAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var substancesArray = substances.OrderBy(g => g.Code, StringComparer.OrdinalIgnoreCase).ToArray();

            var coExposure = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(idi => {
                    var pattern = new BitPattern32(substancesArray.Length);
                    for (int i = 0; i < substancesArray.Length; i++) {
                        if (idi.GetSubstanceTotalExposureCoExposure(substancesArray[i]) > 0) {
                            pattern.Set(i);
                        }
                    }
                    return pattern;
                })
                .ToList();

            var rawGroupedExposurePatterns = CalculateGroupedExposuresPatterns(coExposure, substances);
            var groupedExposurePatterns = GetGroupedExposurePatterns(rawGroupedExposurePatterns);
            AggregatedExposureRecords = GetAggregateRecords(dietaryIndividualDayIntakes.Count, rawGroupedExposurePatterns)
                    .OrderBy(r => r.NumberOfSubstances).ToList();
            UpperFullExposureRecords = groupedExposurePatterns;
            LowerFullExposureRecords = groupedExposurePatterns
                .OrderBy(c => c.NumberOfSubstances)
                .ThenByDescending(c => c.Frequency)
                .ThenBy(c => c.Substances)
                .ToList();
            UpperFullExposureRecordsExtended = GetExposurePatternFrequencies(rawGroupedExposurePatterns, substances);
        }

        protected void SummarizeChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var individualIds = dietaryIndividualDayIntakes
                .Select(c => c.SimulatedIndividualId)
                .Distinct()
                .ToList();
            var substancesArray = substances.OrderBy(g => g.Code, StringComparer.OrdinalIgnoreCase).ToArray();

            var coExposure = dietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .GroupBy(gr => gr.SimulatedIndividualId)
                .Select(days => {
                    var pattern = new BitPattern32(substances.Count);
                    for (int i = 0; i < substancesArray.Length; i++) {
                        var exposure = 0d;
                        foreach (var idi in days) {
                            exposure += idi.GetSubstanceTotalExposureCoExposure(substancesArray[i]);
                        }
                        if (exposure > 0) {
                            pattern.Set(i);
                        }
                    }
                    return pattern;
                })
                .ToList();

            var rawGroupedExposurePatterns = CalculateGroupedExposuresPatterns(coExposure, substances);
            var groupedExposurePatterns = GetGroupedExposurePatterns(rawGroupedExposurePatterns);
            AggregatedExposureRecords = GetAggregateRecords(individualIds.Count, rawGroupedExposurePatterns)
                    .OrderBy(r => r.NumberOfSubstances).ToList();
            var hiddenProperties = new List<string> { };
            UpperFullExposureRecords = groupedExposurePatterns;
            LowerFullExposureRecords = groupedExposurePatterns
                .OrderBy(c => c.NumberOfSubstances)
                .ThenByDescending(c => c.Frequency)
                .ThenBy(c => c.Substances)
                .ToList();
            UpperFullExposureRecordsExtended = GetExposurePatternFrequencies(rawGroupedExposurePatterns, substances);
        }
    }
}
