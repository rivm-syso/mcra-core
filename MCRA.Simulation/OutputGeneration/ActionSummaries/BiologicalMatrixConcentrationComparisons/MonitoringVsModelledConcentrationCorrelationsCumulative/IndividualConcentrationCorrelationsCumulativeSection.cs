using MathNet.Numerics.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class IndividualConcentrationCorrelationsCumulativeSection : SummarySection {

        public List<DayConcentrationCorrelationsBySubstanceRecord> Records { get; set; } = [];

        public TargetUnit TargetUnit { get; set; }
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> targetExposures,
            ICollection<HbmIndividualCollection> collections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            var result = new List<DayConcentrationCorrelationsBySubstanceRecord>();
            foreach (var collection in collections) {
                TargetUnit = collection.TargetUnit;

                var cumulativeExposures = targetExposures
                    .Select(r => (
                        SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                        TargetExposure: r,
                        Exposure: r.GetTotalExposureAtTarget(targetUnit.Target, relativePotencyFactors, membershipProbabilities)
                        ))
                    .ToList();

                var concentrations = collection.HbmIndividualConcentrations
                    .Select(r => (
                        individual: r.SimulatedIndividual,
                        concentration: substances.Sum(
                            substance => r.GetSubstanceExposure(substance) * relativePotencyFactors[substance]
                             * collection.TargetUnit.GetAlignmentFactor(targetUnit, substance.MolecularMass, double.NaN)
                        )
                    ))
                    .ToList();
                var substance = new Compound() { Code = "Cumulative" };
                var record = new DayConcentrationCorrelationsBySubstanceRecord() {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    MonitoringVersusModelExposureRecords = []
                };

                var modelledExposuresLookup = cumulativeExposures.ToLookup(r => r.TargetExposure.SimulatedIndividual.Code);
                record.UnmatchedMonitoringConcentrations = 0;

                foreach (var (individual, concentration) in concentrations) {
                    var matchedModelRecords = modelledExposuresLookup.Contains(individual.Code) ? modelledExposuresLookup[individual.Code] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.SimulatedIndividual.Code,
                                ModelledExposure = modelledExposure.Exposure,
                                MonitoringConcentration = concentration
                            })
                            .ToList();
                        record.MonitoringVersusModelExposureRecords.AddRange(monitoringVersusModelExposureRecords);
                    } else {
                        record.UnmatchedMonitoringConcentrations++;
                    }
                }

                if (record.MonitoringVersusModelExposureRecords.Any()) {
                    var bothPositiveRecords = record.MonitoringVersusModelExposureRecords
                        .Where(r => r.ModelledExposure > 0 && r.MonitoringConcentration > 0)
                        .ToList();
                    if (bothPositiveRecords.Any()) {
                        record.Pearson = Correlation.Spearman(
                            bothPositiveRecords.Select(r => Math.Log10(r.ModelledExposure)),
                            bothPositiveRecords.Select(r => Math.Log10(r.MonitoringConcentration))
                        );
                    }
                    record.Spearman = Correlation.Spearman(
                        record.MonitoringVersusModelExposureRecords.Select(r => r.ModelledExposure),
                        record.MonitoringVersusModelExposureRecords.Select(r => r.MonitoringConcentration)
                    );
                }

                var matchedIndividualDays = record.MonitoringVersusModelExposureRecords
                    .Select(r => r.Individual)
                    .ToHashSet();
                record.UnmatchedModelExposures = cumulativeExposures
                    .Count(r => !matchedIndividualDays.Contains(r.TargetExposure.SimulatedIndividual.Code));
                result.Add(record);

                Records.AddRange(result);
            }
        }
    }
}
