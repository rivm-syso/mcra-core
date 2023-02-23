using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AnalyticalMethodsSummarySection : SummarySection {

        public List<AnalyticalMethodSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<FoodSample> foodSamples, ICollection<Compound> selectedCompounds) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();

            var compoundsLookup = selectedCompounds.ToHashSet();
            var records = foodSamples.SelectMany(c => c.SampleAnalyses)
                .Where(s => s.AnalyticalMethod?.AnalyticalMethodCompounds?.Any(amc => compoundsLookup.Contains(amc.Key)) ?? false)
                .GroupBy(r => r.AnalyticalMethod)
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(r => {
                    var amcs = r.Key. AnalyticalMethodCompounds.Values.OrderBy(amc => amc.Compound.Name, StringComparer.OrdinalIgnoreCase).ToList();
                    return new AnalyticalMethodSummaryRecord() {
                        AnalyticalMethodName = r.Key.Name,
                        AnalyticalMethodCode = r.Key.Code,
                        NumberOfSamples = r.Count(),
                        SubstanceCodes = amcs.Select(amc => amc.Compound.Code).ToList(),
                        SubstanceNames = amcs.Select(amc => amc.Compound.Name).ToList(),
                        Lods = amcs.Select(amc => amc.LOD).ToList(),
                        Loqs = amcs.Select(amc => amc.LOQ).ToList(),
                        ConcentrationUnits = amcs.Select(amc => amc.GetConcentrationUnit().GetShortDisplayName()).ToList(),
                    };
                })
                .OrderBy(r => r.AnalyticalMethodName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            Records = records;
        }
    }
}
