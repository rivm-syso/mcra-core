using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UseFrequenciesSection : SummarySection {
        public List<UseFrequenciesSummaryRecord> Records { get; set; }

        public void Summarize(
            ICollection<IndividualConsumerProductUseFrequency> consumerProductUseFrequencies,
            ICollection<Individual> individuals,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };

            Records = [.. consumerProductUseFrequencies
                .Where(c => individuals.Contains(c.Individual))
                .GroupBy(c => c.Product)
                .Select(c => {
                    var frequencies = c.Select(c => c.Frequency).ToList();
                    var weights = c.Select(c => c.Individual.SamplingWeight).ToList();
                    var percentiles = frequencies.PercentilesWithSamplingWeights(weights, percentages);
                    return new UseFrequenciesSummaryRecord {
                        Code = c.Key.Code,
                        Name = c.Key.Name,
                        NumberOfIndividualDays = individuals.Count,
                        PercentageOfIndividualDaysWithUse = c.Count(r => r.Frequency > 0) *100d/individuals.Count,
                        Mean = frequencies.Average(),
                        Median = percentiles[1],
                        Percentile25All = percentiles[0],
                        Percentile75All = percentiles[2]
                    };
                })
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)];
        }
    }
}
