using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DayConcentrationCorrelationsBySubstanceSection : SummarySection {

        private const char _sep = '\a';

        public List<IndividualConcentrationCorrelationsBySubstanceRecord> Records { get; set; }
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }

        public void Summarize(
            ICollection<ITargetIndividualDayExposure> targetExposures,
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
            ICollection<Compound> substances,
            TargetUnit targetExposureUnit,
            TargetUnit hbmConcentrationUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            var result = new List<IndividualConcentrationCorrelationsBySubstanceRecord>();
            foreach (var substance in substances) {

                var substanceTargetExposures = targetExposures
                    .Select(r => (
                        TargetExposure: r,
                        CompoundExposures: r.GetSubstanceTargetExposure(substance) as ISubstanceTargetExposure
                    ))
                    .ToList();

                var substanceMonitoringConcentrations = hbmIndividualDayConcentrations
                    .Select(r => (
                        MonitoringConcentration: r,
                        CompoundConcentrations: r.AverageEndpointSubstanceExposure(substance)
                            * hbmConcentrationUnit.GetAlignmentFactor(targetExposureUnit, substance.MolecularMass, double.NaN)
                    ))
                    .ToList();

                var record = new IndividualConcentrationCorrelationsBySubstanceRecord() {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                };

                record.MonitoringVersusModelExposureRecords = new List<HbmVsModelledIndividualDayConcentrationRecord>();
                var modelledExposuresLookup = substanceTargetExposures.ToLookup(r => $"{r.TargetExposure.Individual.Code}{_sep}{r.TargetExposure.Day}");
                record.UnmatchedMonitoringConcentrations = 0;
                foreach (var monitoringConcentration in substanceMonitoringConcentrations) {
                    var key = $"{monitoringConcentration.MonitoringConcentration.Individual.Code}{_sep}{monitoringConcentration.MonitoringConcentration.Day}";
                    var matchedModelRecords = modelledExposuresLookup.Contains(key) ? modelledExposuresLookup[key] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualDayConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.Individual.Code,
                                Day = modelledExposure.TargetExposure.Day,
                                ModelledExposure = modelledExposure.CompoundExposures?.SubstanceAmount ?? double.NaN,
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

                var matchedIndividualDays = record.MonitoringVersusModelExposureRecords.ToLookup(r => $"{r.Individual}{_sep}{r.Day}");
                record.UnmatchedModelExposures = substanceTargetExposures.Where(r => !matchedIndividualDays.Contains($"{r.TargetExposure.Individual.Code}{_sep}{r.TargetExposure.Day}")).Count();
                result.Add(record);
            }

            Records = result;
        }
    }
}
