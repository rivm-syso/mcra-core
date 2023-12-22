using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the concentrations of modelled foods from input data
    /// </summary>
    public sealed class ConcentrationDataSummarySection : ActionSummarySectionBase {

        public int TotalNumberOfSamples { get; set; }
        public int TotalNumberOfAnalysedFoods { get; set; }
        public int TotalNumberOfAnalysedSubstances { get; set; }

        public void Summarize(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            ICollection<Compound> substances
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var allSampleCompoundRecords = sampleCompoundCollections.SelectMany(r => r.SampleCompoundRecords).ToList();
            TotalNumberOfSamples = allSampleCompoundRecords?.Count ?? 0;
            TotalNumberOfAnalysedSubstances = substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .Count(c => allSampleCompoundRecords.Any(r => r.SampleCompounds.ContainsKey(c) && !r.SampleCompounds[c].IsMissingValue));
            TotalNumberOfAnalysedFoods = sampleCompoundCollections.Select(r => r.Food)
                .Count();
        }
    }
}
