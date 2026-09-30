using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualTimePointDistributionsSection
        : HbmConcentrationByDescriptorSectionBase<HbmSubstanceTimePointContributorKey, HbmSubstanceTimePointDistributionRecord, HbmSubstanceTimePointPercentilesRecord> {
        
        public void Summarize(
            HbmCumulativeIndividualDayCollection collection,
            PopulationStratifier stratifier,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs
        ) {
            ExposureType = ExposureType.Acute;
            StratificationVariables = stratifier?.GetStratificationVariables();
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(collection.IndividualDayConcentrations.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var substance = new Compound() { Name = "Cumulative", Code = "Cumulative" };
            var groupedByTimepoints = collection.IndividualDayConcentrations
                .GroupBy(c => c.TimePoint)
                .Select(g => new {
                    Timepoint = g.Key,
                    Concentrations = g.ToList()
                })
                .ToList();

            var stratifiedHbmBoxPlotRecords = new List<HbmSubstanceTimePointPercentilesRecord>();
            var hbmBoxPlotRecords = new List<HbmSubstanceTimePointPercentilesRecord>();
            foreach (var group in groupedByTimepoints) {
                var timepointCollection = new HbmCumulativeIndividualDayCollection {
                    TargetUnit = collection.TargetUnit,
                    IndividualDayConcentrations = group.Concentrations
                };
                var descriptor = new HbmSubstanceTimePointContributorKey() { Substance = substance, TimePoint = group.Timepoint };

                var concentrations = getConcentrations(timepointCollection, stratifier);
                var record = CreateSummaryRecord(
                    concentrations,
                    descriptor,
                    collection.TargetUnit,
                    null,
                    percentages
                );
                Records.Add(record);

                if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                    var stratifiedRecords = CreateStratifiedSummaryRecords(
                        concentrations,
                        descriptor,
                        collection.TargetUnit,
                        percentages,
                        true
                    );
                    Records.AddRange(stratifiedRecords);
                }

                var result = CreateBoxPlotRecord(
                    concentrations,
                    descriptor,
                    null,
                    collection.TargetUnit
                );
                hbmBoxPlotRecords.Add(result);

                if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                    var stratifiedResults = CreateStratifiedBoxPlotRecords(
                        concentrations,
                        descriptor,
                        collection.TargetUnit
                    );
                    stratifiedHbmBoxPlotRecords.AddRange(stratifiedResults);
                }

            }
            HbmBoxPlotRecords[collection.TargetUnit.Target] = [.. hbmBoxPlotRecords.OrderByDescending(r => r.GetDescriptorKey())];
            StratifiedHbmBoxPlotRecords[collection.TargetUnit.Target] = [.. stratifiedHbmBoxPlotRecords.OrderByDescending(r => r.GetDescriptorKey())];
        }

        public void SummarizeUncertainty(
            HbmCumulativeIndividualDayCollection collection,
            PopulationStratifier stratifier,
            double lowerBound,
            double upperBound
        ) {
            var groupedByTimepoints = collection.IndividualDayConcentrations
                .GroupBy(c => c.TimePoint)
                .Select(g => new {
                    Timepoint = g.Key,
                    Concentrations = g.ToList()
                })
                .ToList();
            var substance = new Compound() { Name = "Cumulative", Code = "Cumulative" };
            foreach (var group in groupedByTimepoints) {
                var timepointCollection = new HbmCumulativeIndividualDayCollection {
                    TargetUnit = collection.TargetUnit,
                    IndividualDayConcentrations = group.Concentrations
                };
                var descriptor = new HbmSubstanceTimePointContributorKey() { Substance = substance, TimePoint = group.Timepoint };

                var hbmConcentrations = getConcentrations(timepointCollection, stratifier);
                UpdateRecord(
                    hbmConcentrations,
                    descriptor,
                    collection.TargetUnit.Target,
                    null,
                    lowerBound,
                    upperBound
                );
                if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                    UpdateStratifiedRecords(
                        hbmConcentrations,
                        descriptor,
                        collection.TargetUnit.Target,
                        lowerBound,
                        upperBound
                    );
                }
            }
        }

        private static List<HbmConcentrationsByDescriptor<HbmSubstanceTimePointContributorKey>> getConcentrations(
            HbmCumulativeIndividualDayCollection collection,
            PopulationStratifier stratifier
        ) {
            var concentrations = collection.IndividualDayConcentrations
                .Select(c => new HbmConcentrationsByDescriptor<HbmSubstanceTimePointContributorKey> {
                    SamplingWeight = c.SimulatedIndividual.SamplingWeight,
                    TotalEndpointExposure = c.CumulativeConcentration,
                    StratificationLevel = stratifier?.GetLevel(c.SimulatedIndividual),
                    SourceSamplingMethods = []
                })
                .ToList();
            return concentrations;
        }
    }
}