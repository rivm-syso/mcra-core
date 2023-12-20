using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualContributionsSection : HbmContributionsSectionBase {
        public void SummarizeBoxPlots(
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var exposureSumByIndividual = collection
                .HbmIndividualConcentrations
                .Select(c => (
                    Sum: c.ConcentrationsBySubstance.Values.Sum(s => s.Concentration * relativePotencyFactors[s.Substance]),
                    SimulatedIndividualId: c.SimulatedIndividualId
                ))
                .ToDictionary(c => c.SimulatedIndividualId, c => c.Sum);

            var samplingWeights = collection
                .HbmIndividualConcentrations
                .Select(c => c.Individual.SamplingWeight).ToList();

            foreach (var substance in substances) {
                var individualContributions = collection.HbmIndividualConcentrations
                    .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                    .Select(c => {
                        return c.GetExposureForSubstance(substance) 
                            / exposureSumByIndividual[c.SimulatedIndividualId] 
                            * relativePotencyFactors[substance] * 100;
                    })
                    .ToList();
                if (individualContributions.Any()) {
                    var (boxPlotRecord, contributionRecord) = getBoxPlotRecord(
                        collection.Target,
                        samplingWeights,
                        substance,
                        individualContributions
                     );
                    IndividualContributionRecords.Add(contributionRecord);
                    HbmBoxPlotRecords.Add(boxPlotRecord);
                }
            }
        }

        public void SummarizeUncertain(
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            double lowerBound,
            double upperBound
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var exposureSumByIndividual = collection
                .HbmIndividualConcentrations
                .Select(c => (
                    Sum: c.ConcentrationsBySubstance.Values.Sum(s => s.Concentration * relativePotencyFactors[s.Substance]),
                    SimulatedIndividualId: c.SimulatedIndividualId
                ))
                .ToDictionary(c => c.SimulatedIndividualId, c => c.Sum);

            var samplingWeights = collection
                .HbmIndividualConcentrations
                .Select(c => c.Individual.SamplingWeight).ToList();

            foreach (var substance in substances) {
                var individualContributions = collection.HbmIndividualConcentrations
                    .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                    .Select(c => {
                        var sum = exposureSumByIndividual[c.SimulatedIndividualId];
                        return sum != 0 ? c.GetExposureForSubstance(substance) / sum * relativePotencyFactors[substance] * 100 : 0;
                    })
                    .ToList();
                var meanContribution = individualContributions.Zip(samplingWeights, (i, w) => i * w).Sum() / samplingWeights.Sum();
                var record = IndividualContributionRecords
                    .Where(c => c.SubstanceCode == substance.Code && c.TargetUnit == collection.Target)
                    .SingleOrDefault();
                if (record != null) {
                    record.Contributions.Add(meanContribution);
                    record.UncertaintyLowerBound = lowerBound;
                    record.UncertaintyUpperBound = upperBound;
                }
            }
        }
    }
}