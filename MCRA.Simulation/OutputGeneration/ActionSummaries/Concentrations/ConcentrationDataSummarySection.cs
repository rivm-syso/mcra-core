using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the concentrations of modelled foods from input data
    /// </summary>
    public sealed class ConcentrationDataSummarySection : ActionSummaryBase {

        public int TotalNumberOfSamples { get; set; }
        public int TotalNumberOfAnalysedFoods { get; set; }
        public int TotalNumberOfAnalysedSubstances { get; set; }

        public void Summarize(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            ICollection<Compound> substances
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var allSampleCompoundRecords = sampleCompoundCollections.SelectMany(r => r.SampleCompoundRecords).ToList();
            TotalNumberOfSamples = allSampleCompoundRecords?.Count ?? 0;
            TotalNumberOfAnalysedSubstances = substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .Where(c => allSampleCompoundRecords.Where(r => r.SampleCompounds.ContainsKey(c) && !r.SampleCompounds[c].IsMissingValue).Any())
                .Count();
            TotalNumberOfAnalysedFoods = sampleCompoundCollections.Select(r => r.Food)
                .Count();
        }
    }
}
