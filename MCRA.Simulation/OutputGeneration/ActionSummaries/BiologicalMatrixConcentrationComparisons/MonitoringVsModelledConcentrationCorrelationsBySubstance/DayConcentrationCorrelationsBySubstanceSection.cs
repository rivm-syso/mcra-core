using MathNet.Numerics.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DayConcentrationCorrelationsBySubstanceSection : SummarySection {

        private const char Sep = '\a';

        public List<IndividualConcentrationCorrelationsBySubstanceRecord> Records { get; set; } = [];
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }
        public TargetUnit TargetUnit { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualDayExposure> targetExposures,
            ICollection<HbmIndividualDayCollection> collections,
            ICollection<Compound> substances,
            TargetUnit targetUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            TargetUnit = collections.FirstOrDefault().TargetUnit;
            foreach (var substance in substances) {
                var exposures = targetExposures
                    .Select(r => (
                        TargetExposure: r,
                        Exposures: r.GetSubstanceTargetExposure(targetUnit.Target, substance)
                    ))
                    .ToList();

                var monitoringConcentrations = collections
                    .SelectMany(r => r.HbmIndividualDayConcentrations, (c, r) => (targetUnit: c.TargetUnit, hbmConcentration: r))
                    .Select(r => (
                        individual: r.hbmConcentration.SimulatedIndividual,
                        concentration: r.hbmConcentration.AverageEndpointSubstanceExposure(substance)
                            * r.targetUnit.GetAlignmentFactor(targetUnit, substance.MolecularMass, double.NaN),
                        day: r.hbmConcentration.Day
                    ))
                    .ToList();

                var record = new IndividualConcentrationCorrelationsBySubstanceRecord {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    MonitoringVersusModelExposureRecords = []
                };

                var modelledExposuresLookup = exposures.ToLookup(r => $"{r.TargetExposure.SimulatedIndividual.Code}{Sep}{r.TargetExposure.Day}");
                record.UnmatchedMonitoringConcentrations = 0;

                foreach (var (individual, concentration, day) in monitoringConcentrations) {
                    var key = $"{individual.Code}{Sep}{day}";
                    var matchedModelRecords = modelledExposuresLookup.Contains(key) ? modelledExposuresLookup[key] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualDayConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.SimulatedIndividual.Code,
                                Day = modelledExposure.TargetExposure.Day,
                                ModelledExposure = modelledExposure.Exposures?.Exposure ?? double.NaN,
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
                            ModelledExposure: g.Select(r => r.ModelledExposure).Median(),
                            MonitoringConcentration: g.Select(r => r.MonitoringConcentration).Average()
                        )).ToList();
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

                var matchedIndividualDays = record.MonitoringVersusModelExposureRecords
                    .Select(r => $"{r.Individual}{Sep}{r.Day}")
                    .ToHashSet();
                record.UnmatchedModelExposures = exposures
                    .Count(r => !matchedIndividualDays.Contains($"{r.TargetExposure.SimulatedIndividual.Code}{Sep}{r.TargetExposure.Day}"));
                Records.Add(record);
            }
        }
    }
}
