using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualDayContributionsSection : HbmContributionsSectionBase {
        public void SummarizeBoxPlots(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var exposureSumByIndividual = collection
                .HbmIndividualDayConcentrations
                .Select(c => (
                    Sum: c.ConcentrationsBySubstance.Values.Sum(s => s.Concentration * relativePotencyFactors[s.Substance]),
                    SimulatedIndividualDayId: c.SimulatedIndividualDayId
                ))
                .ToDictionary(c => c.SimulatedIndividualDayId, c => c.Sum);

            var samplingWeights = collection
                .HbmIndividualDayConcentrations
                .Select(c => c.Individual.SamplingWeight).ToList();

            foreach (var substance in substances) {
                var individualContributions = collection.HbmIndividualDayConcentrations
                    .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                    .Select(c => {
                        var sum = exposureSumByIndividual[c.SimulatedIndividualDayId];
                        return sum != 0 ? c.GetExposureForSubstance(substance) / sum * relativePotencyFactors[substance] * 100 : 0;
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
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            double lowerBound,
            double upperBound
        ) {
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var exposureSumByIndividual = collection
                .HbmIndividualDayConcentrations
                .Select(c => (
                    Sum: c.ConcentrationsBySubstance.Values.Sum(s => s.Concentration * relativePotencyFactors[s.Substance]),
                    SimulatedIndividualDayId: c.SimulatedIndividualDayId
                ))
                .ToDictionary(c => c.SimulatedIndividualDayId, c => c.Sum);

            var samplingWeights = collection
                .HbmIndividualDayConcentrations
                .Select(c => c.Individual.SamplingWeight).ToList();

            foreach (var substance in substances) {
                var individualContributions = collection.HbmIndividualDayConcentrations
                    .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                    .Select(c => {
                        var sum = exposureSumByIndividual[c.SimulatedIndividualDayId];
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
