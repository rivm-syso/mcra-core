using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public class CoExposureDistributionSectionBase : SummarySection {

        public List<FullCoExposureRecord> UpperFullExposureRecords { get; set; }
        public List<FullCoExposureRecord> LowerFullExposureRecords { get; set; }
        public List<FullCoExposureRecord> UpperFullExposureRecordsExtended { get; set; }
        public List<CoExposureRecord> AggregatedExposureRecords { get; set; }

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
            UpperFullExposureRecordsExtended = getExposurePatternFrequencies(
                rawGroupedExposurePatterns, 
                substances
            );
        }

        protected void SummarizeAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
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
            UpperFullExposureRecordsExtended = getExposurePatternFrequencies(rawGroupedExposurePatterns, substances);
        }

        protected void SummarizeChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
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
            UpperFullExposureRecordsExtended = getExposurePatternFrequencies(rawGroupedExposurePatterns, substances);
        }

        /// <summary>
        /// Group by number of substances
        /// </summary>
        /// <param name="numberOfIntakes"></param>
        /// <param name="fullExposureRecords"></param>
        /// <returns></returns>
        public List<CoExposureRecord> GetAggregateRecords(int numberOfIntakes, List<DetailCoExposureRecord> fullExposureRecords) {
            return fullExposureRecords
                .GroupBy(gr => gr.NumberOfSubstances)
                .Select(c => {
                    var frequency = c.Sum(g => g.Frequency);
                    return new CoExposureRecord() {
                        NumberOfSubstances = c.Key,
                        Frequency = frequency,
                        Percentage = 100D * frequency / numberOfIntakes,
                    };
                })
                .OrderBy(c => c.NumberOfSubstances)
                .ThenByDescending(c => c.Percentage)
                .ToList();
        }

        /// <summary>
        /// Extracts relevant records
        /// </summary>
        /// <param name="groupedExposurePatterns"></param>
        /// <returns></returns>
        public List<FullCoExposureRecord> GetGroupedExposurePatterns(List<DetailCoExposureRecord> groupedExposurePatterns) {
            return groupedExposurePatterns.Select(c => new FullCoExposureRecord() {
                Frequency = c.Frequency,
                Substances = c.Substances,
                NumberOfSubstances = c.NumberOfSubstances,
                Percentage = c.Percentage,
            }).ToList();
        }

        /// <summary>
        /// Group all exposures based on co-occurrence patterns for substances
        /// </summary>
        /// <param name="exposurePatterns"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public List<DetailCoExposureRecord> CalculateGroupedExposuresPatterns(List<BitPattern32> exposurePatterns, ICollection<Compound> substances) {
            return exposurePatterns
                .AsParallel()
                .GroupBy(gr => gr)
                .Select((c, row) => {
                    var binaryPattern = c.Key;
                    var frequency = c.Count();
                    var indexArray = binaryPattern.IndicesOfSetBits.ToArray();
                    return new DetailCoExposureRecord {
                        Binary = binaryPattern,
                        Frequency = frequency,
                        NumberOfSubstances = binaryPattern.NumberOfSetBits,
                        Index = indexArray,
                        Row = row,
                        Substances = getSubstanceNames(indexArray, substances),
                        Percentage = 100D * frequency / exposurePatterns.Count,
                    };
                })
                .OrderByDescending(c => c.NumberOfSubstances)
                .ThenByDescending(c => c.Percentage)
                .ThenBy(c => c.Substances)
                .ToList();
        }

        /// <summary>
        /// Calculate the contribution of patterns containing the combination of patterns
        /// </summary>
        /// <param name="groupedExposurePatterns"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        private List<FullCoExposureRecord> getExposurePatternFrequencies(List<DetailCoExposureRecord> groupedExposurePatterns, ICollection<Compound> substances) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken };

            var groupedExposurePatternsCount = groupedExposurePatterns.Sum(c => c.Frequency);
            var results = new FullCoExposureRecord[groupedExposurePatterns.Count];

            //create an array of arrays grouped by number of substances, ordered descending by number of substances
            var groupedRecords = groupedExposurePatterns
                .GroupBy(p => p.NumberOfSubstances)
                .OrderBy(g => g.Key)
                .Select(g => g.ToArray())
                .ToArray();

            //next steps, fill all records with one order lower number of substances and sum frequencies
            Parallel.For(0, groupedRecords.Length, parallelOptions, i => {
                var editGroup = groupedRecords[i];
                //iterate over rest of groups, this can be done parallel
                Parallel.For(0, editGroup.Length, parallelOptions, j => {
                    var editItem = editGroup[j];
                    var frequency = editItem.Frequency;
                    //loop over all sets with length > editgroup
                    if (editItem.NumberOfSubstances > 0) {
                        for (int l = i + 1; l < groupedRecords.Length; l++) {
                            //loop over individual items
                            frequency += groupedRecords[l]
                                .Where(r => editItem.Binary.IsSubSetOf(r.Binary))
                                .Sum(r => r.Frequency);
                        }
                    }
                    results[editItem.Row] = new FullCoExposureRecord() {
                        Frequency = frequency,
                        Substances = getSubstanceNames(editItem.Index, substances),
                        NumberOfSubstances = editItem.NumberOfSubstances,
                        Percentage = 100D * frequency / groupedExposurePatternsCount
                    };
                });
            });

            return results.OrderByDescending(c => c.Percentage).ThenBy(c => c.Substances).ToList();
        }

        private static string getSubstanceNames(int[] index, ICollection<Compound> substances) {
            var result = string.Join(", ", index.Select(i => substances.ElementAt(i).Name));
            return result;
        }
    }
}
