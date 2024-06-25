using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DayConcentrationCorrelationsCumulativeSection : SummarySection {

        private const char _sep = '\a';

        public List<IndividualConcentrationCorrelationsBySubstanceRecord> Records { get; set; } = new();
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }
        public string ExposureTarget { get; set; }
        public void Summarize(
            ICollection<ITargetIndividualDayExposure> targetExposures,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetExposureUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            var result = new List<IndividualConcentrationCorrelationsBySubstanceRecord>();
            //TODO for collections targetUnit
            foreach (var collection in hbmIndividualDayCollections) {
                ExposureTarget = collection.TargetUnit.ExposureUnit.GetShortDisplayName();
                var cumulativeTargetExposures = targetExposures
                    .Select(r => (
                        TargetExposure: r,
                        SubstanceExposure: r.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, false)
                        ))
                    .ToList();


                var concentrationAlignmentFactor = collection.TargetUnit.GetAlignmentFactor(targetExposureUnit, double.NaN, double.NaN);

                var cumulativeMonitoringConcentrations = collection.HbmIndividualDayConcentrations
                    .Select(r => (
                        MonitoringConcentration: r,
                        CompoundConcentrations: substances.Sum(
                            substance => r.AverageEndpointSubstanceExposure(substance)
                                * relativePotencyFactors[substance]
                                * concentrationAlignmentFactor
                        )
                    ))
                    .ToList();

                var record = new IndividualConcentrationCorrelationsBySubstanceRecord() {
                    SubstanceCode = "Cumulative",
                    SubstanceName = "Cumulative",
                };

                record.MonitoringVersusModelExposureRecords = new List<HbmVsModelledIndividualDayConcentrationRecord>();
                var modelledExposuresLookup = cumulativeTargetExposures.ToLookup(r => $"{r.TargetExposure.Individual.Code}{_sep}{r.TargetExposure.Day}");
                record.UnmatchedMonitoringConcentrations = 0;
                foreach (var monitoringConcentration in cumulativeMonitoringConcentrations) {
                    var key = $"{monitoringConcentration.MonitoringConcentration.Individual.Code}{_sep}{monitoringConcentration.MonitoringConcentration.Day}";
                    var matchedModelRecords = modelledExposuresLookup.Contains(key) ? modelledExposuresLookup[key] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualDayConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.Individual.Code,
                                Day = modelledExposure.TargetExposure.Day,
                                ModelledExposure = modelledExposure.SubstanceExposure,
                                MonitoringConcentration = monitoringConcentration.CompoundConcentrations
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
                        .Select(g => {
                            var modelled = g.Select(r => r.ModelledExposure).ToList();
                            var monitoring = g.Select(r => r.MonitoringConcentration).Average();
                            var modelledMedian = modelled.Median();
                            return (
                                Monitoring: monitoring,
                                ModelledMedian: modelledMedian
                            );
                        })
                        .ToList();
                    var bothPositiveRecords = values
                        .Where(r => r.ModelledMedian > 0 && r.Monitoring > 0)
                        .ToList();
                    if (bothPositiveRecords.Any()) {
                        record.Pearson = MathNet.Numerics.Statistics.Correlation.Spearman(
                            bothPositiveRecords.Select(r => Math.Log10(r.ModelledMedian)),
                            bothPositiveRecords.Select(r => Math.Log10(r.Monitoring))
                        );
                    }
                    record.Spearman = MathNet.Numerics.Statistics.Correlation.Spearman(
                        values.Select(r => r.ModelledMedian),
                        values.Select(r => r.Monitoring)
                    );
                }

                var matchedIndividualDays = record
                    .MonitoringVersusModelExposureRecords
                    .Select(r => $"{r.Individual}{_sep}{r.Day}")
                    .ToHashSet();
                record.UnmatchedModelExposures = cumulativeTargetExposures
                    .Count(r => !matchedIndividualDays.Contains($"{r.TargetExposure.Individual.Code}{_sep}{r.TargetExposure.Day}"));
                result.Add(record);

                Records.AddRange(result);
            }
        }
    }
}
