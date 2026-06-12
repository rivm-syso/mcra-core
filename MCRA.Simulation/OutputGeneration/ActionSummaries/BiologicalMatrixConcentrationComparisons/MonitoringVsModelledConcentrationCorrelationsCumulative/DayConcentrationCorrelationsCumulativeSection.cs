using MathNet.Numerics.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DayConcentrationCorrelationsCumulativeSection : SummarySection {

        private const char Sep = '\a';

        public List<IndividualConcentrationCorrelationsBySubstanceRecord> Records { get; set; } = [];
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public void Summarize(
            ICollection<AggregateIndividualDayExposure> targetExposures,
            ICollection<HbmIndividualDayCollection> collections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            var result = new List<IndividualConcentrationCorrelationsBySubstanceRecord>();
            foreach (var collection in collections) {
                TargetUnit = collection.TargetUnit;

                var cumulativeExposures = targetExposures
                    .Select(r => (
                        TargetExposure: r,
                        Exposure: r.GetTotalExposureAtTarget(targetUnit.Target, relativePotencyFactors, membershipProbabilities)
                        ))
                    .ToList();

                var concentrationAlignmentFactor = collection.TargetUnit.GetAlignmentFactor(targetUnit, double.NaN, double.NaN);

                var concentrations = collection.HbmIndividualDayConcentrations
                    .Select(r => (
                        MonitoringConcentration: r,
                        Concentration: substances.Sum(
                            substance => r.AverageEndpointSubstanceExposure(substance)
                                * relativePotencyFactors[substance]
                                * concentrationAlignmentFactor
                        )
                    ))
                    .ToList();
                var substance = new Compound() { Code = "Cumulative" };
                var record = new IndividualConcentrationCorrelationsBySubstanceRecord {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    MonitoringVersusModelExposureRecords = []
                };
                var modelledExposuresLookup = cumulativeExposures.ToLookup(r => $"{r.TargetExposure.SimulatedIndividual.Code}{Sep}{r.TargetExposure.Day}");
                record.UnmatchedMonitoringConcentrations = 0;
                foreach (var (monitoringConcentration, concentration) in concentrations) {
                    var key = $"{monitoringConcentration.SimulatedIndividual.Code}{Sep}{monitoringConcentration.Day}";
                    var matchedModelRecords = modelledExposuresLookup.Contains(key) ? modelledExposuresLookup[key] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualDayConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.SimulatedIndividual.Code,
                                Day = modelledExposure.TargetExposure.Day,
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
                    var values = record.MonitoringVersusModelExposureRecords
                        .GroupBy(r => (r.Individual, r.Day))
                        .Select(g => (
                                MonitoringConcentration: g.Select(r => r.MonitoringConcentration).Average(),
                                ModelledExposure: g.Select(r => r.ModelledExposure).Median()
                            )
                        ).ToList();
                    var bothPositiveRecords = values
                        .Where(r => r.ModelledExposure > 0 && r.MonitoringConcentration > 0)
                        .ToList();
                    if (bothPositiveRecords.Any()) {
                        record.Pearson = Correlation.Spearman(
                            bothPositiveRecords.Select(r => Math.Log10(r.ModelledExposure)),
                            bothPositiveRecords.Select(r => Math.Log10(r.MonitoringConcentration))
                        );
                    }
                    record.Spearman = Correlation.Spearman(
                        values.Select(r => r.ModelledExposure),
                        values.Select(r => r.MonitoringConcentration)
                    );
                }

                var matchedIndividualDays = record
                    .MonitoringVersusModelExposureRecords
                    .Select(r => $"{r.Individual}{Sep}{r.Day}")
                    .ToHashSet();
                record.UnmatchedModelExposures = cumulativeExposures
                    .Count(r => !matchedIndividualDays.Contains($"{r.TargetExposure.SimulatedIndividual.Code}{Sep}{r.TargetExposure.Day}"));
                result.Add(record);

                Records.AddRange(result);
            }
        }
    }
}
