using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class IndividualConcentrationCorrelationsBySubstanceSection : SummarySection {

        public List<DayConcentrationCorrelationsBySubstanceRecord> Records { get; set; }
        public List<BiologicalMatrixConcentrationPercentilesRecord> HbmBoxPlotRecords { get; set; }
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }
        public string ExposureTarget { get; set; }
        public void Summarize(
            ICollection<ITargetIndividualExposure> targetExposures,
            ICollection<HbmIndividualCollection> hbmIndividualConcentrationsCollections,
            ICollection<Compound> substances,
            TargetUnit targetExposureUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            //TODO
            ExposureTarget = hbmIndividualConcentrationsCollections.FirstOrDefault().TargetUnit.ExposureUnit.GetShortDisplayName();
            var result = new List<DayConcentrationCorrelationsBySubstanceRecord>();
            foreach (var substance in substances) {

                var compoundTargetExposures = targetExposures
                    .Select(r => (
                        TargetExposure: r,
                        CompoundExposures: r.GetSubstanceTargetExposure(substance)
                    ))
                    .ToList();
                var substanceHbmConcentrations = hbmIndividualConcentrationsCollections
                   .SelectMany(r => r.HbmIndividualConcentrations, (c, r) => (targetUnit: c.TargetUnit, hbmConcentration: r))
                   .Select(r => (
                       individual: r.hbmConcentration.SimulatedIndividual,
                       substanceConcentration: r.hbmConcentration.ConcentrationsBySubstance[substance].Exposure
                           * r.targetUnit.GetAlignmentFactor(targetExposureUnit, substance.MolecularMass, double.NaN)
                   ))
                   .ToList();

                var record = new DayConcentrationCorrelationsBySubstanceRecord() {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                };

                record.MonitoringVersusModelExposureRecords = [];
                var modelledExposuresLookup = compoundTargetExposures.ToLookup(r => r.TargetExposure.SimulatedIndividual.Code);
                record.UnmatchedMonitoringConcentrations = 0;

                foreach (var (individual, substanceConcentration) in substanceHbmConcentrations) {
                    var matchedModelRecords = modelledExposuresLookup.Contains(individual.Code) ? modelledExposuresLookup[individual.Code] : null;
                    if (matchedModelRecords?.Any() ?? false) {
                        var monitoringVersusModelExposureRecords = matchedModelRecords
                            .Select(modelledExposure => new HbmVsModelledIndividualConcentrationRecord() {
                                Individual = modelledExposure.TargetExposure.SimulatedIndividual.Code,
                                ModelledExposure = modelledExposure.CompoundExposures?.Exposure ?? double.NaN,
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
                record.UnmatchedModelExposures = compoundTargetExposures
                    .Count(r => !matchedIndividualDays.Contains(r.TargetExposure.SimulatedIndividual.Code));
                result.Add(record);
            }
            Records = result;
        }
    }
}
