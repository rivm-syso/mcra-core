using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class IndividualConcentrationCorrelationsCumulativeSection : SummarySection {

        public List<DayConcentrationCorrelationsBySubstanceRecord> Records { get; set; }
        public List<BiologicalMatrixConcentrationPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }

        public void Summarize(
            ICollection<ITargetIndividualExposure> targetExposures,
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetExposureUnit,
            TargetUnit hbmConcentrationUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            var result = new List<DayConcentrationCorrelationsBySubstanceRecord>();

            var cumulativeTargetExposures = targetExposures
                .Select(r => (
                    TargetExposure: r,
                    CompoundExposures: r.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, false)
                    ))
                    .ToList();

            var cumulativeMonitoringConcentrations = hbmIndividualConcentrations
                .Select(r => (
                    individual: r.Individual,
                    substanceConcentration: substances.Sum(
                        substance => r.GetExposureForSubstance(substance) * relativePotencyFactors[substance]
                         * hbmConcentrationUnit.GetAlignmentFactor(targetExposureUnit, substance.MolecularMass, double.NaN)
                    )
                ))
                .ToList();

            var record = new DayConcentrationCorrelationsBySubstanceRecord() {
                SubstanceCode = "Cumulative",
                SubstanceName = "Cumulative",
            };

            record.MonitoringVersusModelExposureRecords = new List<HbmVsModelledIndividualConcentrationRecord>();
            var modelledExposuresLookup = cumulativeTargetExposures.ToLookup(r => r.TargetExposure.Individual.Code);
            record.UnmatchedMonitoringConcentrations = 0;

            foreach (var item in cumulativeMonitoringConcentrations) {
                var matchedModelRecords = modelledExposuresLookup.Contains(item.individual.Code) ? modelledExposuresLookup[item.individual.Code] : null;
                if (matchedModelRecords?.Any() ?? false) {
                    var monitoringVersusModelExposureRecords = matchedModelRecords
                        .Select(modelledExposure => new HbmVsModelledIndividualConcentrationRecord() {
                            Individual = modelledExposure.TargetExposure.Individual.Code,
                            ModelledExposure = modelledExposure.CompoundExposures,
                            MonitoringConcentration = item.substanceConcentration
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
                    record.Pearson = MathNet.Numerics.Statistics.Correlation.Spearman(
                        bothPositiveRecords.Select(r => Math.Log10(r.ModelledExposure)),
                        bothPositiveRecords.Select(r => Math.Log10(r.MonitoringConcentration))
                    );
                }
                record.Spearman = MathNet.Numerics.Statistics.Correlation.Spearman(
                    record.MonitoringVersusModelExposureRecords.Select(r => r.ModelledExposure),
                    record.MonitoringVersusModelExposureRecords.Select(r => r.MonitoringConcentration)
                );
            }

            var matchedIndividualDays = record.MonitoringVersusModelExposureRecords.ToLookup(r => r.Individual);
            record.UnmatchedModelExposures = cumulativeTargetExposures.Where(r => !matchedIndividualDays.Contains(r.TargetExposure.Individual.Code)).Count();
            result.Add(record);

            Records = result;
        }
    }
}
