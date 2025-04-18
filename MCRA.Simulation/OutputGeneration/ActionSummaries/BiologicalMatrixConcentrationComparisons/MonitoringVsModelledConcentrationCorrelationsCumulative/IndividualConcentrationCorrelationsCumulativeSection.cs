﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class IndividualConcentrationCorrelationsCumulativeSection : SummarySection {

        public List<DayConcentrationCorrelationsBySubstanceRecord> Records { get; set; } = [];
        public List<BiologicalMatrixConcentrationPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public string ExposureTarget { get; set; }
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }

        public void Summarize(
            ICollection<ITargetIndividualExposure> targetExposures,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetExposureUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            var result = new List<DayConcentrationCorrelationsBySubstanceRecord>();
            foreach (var collection in hbmIndividualCollections) {
                ExposureTarget = collection.TargetUnit.ExposureUnit.GetShortDisplayName();
                var cumulativeTargetExposures = targetExposures
                    .Select(r => (
                        TargetExposure: r,
                        CompoundExposures: r.GetCumulativeExposure(relativePotencyFactors, membershipProbabilities)
                        ))
                        .ToList();

                var cumulativeMonitoringConcentrations = collection.HbmIndividualConcentrations
                    .Select(r => (
                        individual: r.SimulatedIndividual,
                        substanceConcentration: substances.Sum(
                            substance => r.GetSubstanceExposure(substance) * relativePotencyFactors[substance]
                             * collection.TargetUnit.GetAlignmentFactor(targetExposureUnit, substance.MolecularMass, double.NaN)
                        )
                    ))
                    .ToList();

                var record = new DayConcentrationCorrelationsBySubstanceRecord() {
                    SubstanceCode = "Cumulative",
                    SubstanceName = "Cumulative",
                    MonitoringVersusModelExposureRecords = []
                };

                var modelledExposuresLookup = cumulativeTargetExposures.ToLookup(r => r.TargetExposure.SimulatedIndividual.Code);
                record.UnmatchedMonitoringConcentrations = 0;

                foreach (var (individual, substanceConcentration) in cumulativeMonitoringConcentrations) {
                    var matchedModelRecords = modelledExposuresLookup.Contains(individual.Code) ? modelledExposuresLookup[individual.Code] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.SimulatedIndividual.Code,
                                ModelledExposure = modelledExposure.CompoundExposures,
                                MonitoringConcentration = substanceConcentration
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

                var matchedIndividualDays = record.MonitoringVersusModelExposureRecords
                    .Select(r => r.Individual)
                    .ToHashSet();
                record.UnmatchedModelExposures = cumulativeTargetExposures
                    .Count(r => !matchedIndividualDays.Contains(r.TargetExposure.SimulatedIndividual.Code));
                result.Add(record);

                Records.AddRange(result);
            }
        }
    }
}
