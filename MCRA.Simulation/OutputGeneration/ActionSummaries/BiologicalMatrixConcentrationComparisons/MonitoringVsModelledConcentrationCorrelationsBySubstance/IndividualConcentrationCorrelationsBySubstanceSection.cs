using MathNet.Numerics.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class IndividualConcentrationCorrelationsBySubstanceSection : SummarySection {

        public List<DayConcentrationCorrelationsBySubstanceRecord> Records { get; set; } = [];
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public void Summarize(
            ICollection<AggregateIndividualExposure> targetExposures,
            ICollection<HbmIndividualCollection> collections,
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
                        SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                        TargetExposure: r,
                        Exposure: r.GetSubstanceTargetExposure(targetUnit.Target, substance)
                        ))
                    .ToList();

                var monitoringConcentrations = collections
                   .SelectMany(r => r.HbmIndividualConcentrations, (c, r) => (targetUnit: c.TargetUnit, hbmConcentration: r))
                   .Select(r => (
                       individual: r.hbmConcentration.SimulatedIndividual,
                       concentration: r.hbmConcentration.ConcentrationsBySubstance[substance].Exposure
                           * r.targetUnit.GetAlignmentFactor(targetUnit, substance.MolecularMass, double.NaN)
                   ))
                   .ToList();

                var record = new DayConcentrationCorrelationsBySubstanceRecord() {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    MonitoringVersusModelExposureRecords = []
                };

                var modelledExposuresLookup = exposures.ToLookup(r => r.TargetExposure.SimulatedIndividual.Code);
                record.UnmatchedMonitoringConcentrations = 0;

                foreach (var (individual, concentration) in monitoringConcentrations) {
                    var matchedModelRecords = modelledExposuresLookup.Contains(individual.Code) ? modelledExposuresLookup[individual.Code] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.SimulatedIndividual.Code,
                                ModelledExposure = modelledExposure.Exposure?.Exposure ?? double.NaN,
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
                record.UnmatchedModelExposures = exposures
                    .Count(r => !matchedIndividualDays.Contains(r.TargetExposure.SimulatedIndividual.Code));
                Records.Add(record);
            }
        }
    }
}
