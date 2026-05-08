using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmIndividualDayDistributionBySubstanceTimePointSection
        : HbmConcentrationByDescriptorSectionBase<HbmSubstanceTimePointContributorKey, HbmSubstanceTimePointDistributionRecord, HbmSubstanceTimePointPercentilesRecord> {

        public void Summarize(
            ICollection<HbmIndividualDayCollection> collections,
            ICollection<Compound> substances,
            PopulationStratifier stratifier,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs,
            bool detailsSection = false
        ) {
            ExposureType = ExposureType.Acute;
            DetailsSection = detailsSection;
            if (skipPrivacySensitiveOutputs) {
                foreach (var collection in collections) {
                    var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(collection.HbmIndividualDayConcentrations.Count);
                    if (_upperWhisker > maxUpperPercentile) {
                        RestrictedUpperPercentile = maxUpperPercentile;
                        break;
                    }
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            var concentrationsAvailable = collections
                .SelectMany(c => c.HbmIndividualDayConcentrations)
                .SelectMany(c => c.ConcentrationsBySubstance)
                .Sum(c => c.Value.Exposure) > 0;

            if (concentrationsAvailable) {
                var percentages = new double[] { lowerPercentage, 50, upperPercentage };
                foreach (var collection in collections) {
                    var stratifiedHbmBoxPlotRecords = new List<HbmSubstanceTimePointPercentilesRecord>();
                    var hbmBoxPlotRecords = new List<HbmSubstanceTimePointPercentilesRecord>();
                    foreach (var substance in substances) {
                        var hbmIndividualDayConcentrations = getConcentrations(collection, substance, stratifier);
                        var timePointGroups = hbmIndividualDayConcentrations.GroupBy(r => r.Descriptor);
                        foreach (var group in timePointGroups) {
                            var record = CreateSummaryRecord(
                                [.. group],
                                group.Key,
                                collection.TargetUnit,
                                percentages,
                                false
                            );
                            Records.Add(record);

                            if (stratifier != null) {
                                var stratifiedRecords = CreateStratifiedSummaryRecords(
                                    [.. group],
                                    group.Key,
                                    collection.TargetUnit,
                                    percentages,
                                    true
                                );
                                Records.AddRange(stratifiedRecords);
                            }

                            var result = CreateBoxPlotRecord(
                                [.. group],
                                group.Key,
                                null,
                                collection.TargetUnit
                            );
                            hbmBoxPlotRecords.Add(result);

                            if (stratifier != null) {
                                var stratifiedResults = CreateStratifiedBoxPlotRecords(
                                    [.. group],
                                    group.Key,
                                    collection.TargetUnit
                                );
                                stratifiedHbmBoxPlotRecords.AddRange(stratifiedResults);
                            }
                        }
                    }

                    HbmBoxPlotRecords[collection.Target] = hbmBoxPlotRecords;
                    StratifiedHbmBoxPlotRecords[collection.Target] = stratifiedHbmBoxPlotRecords;
                }
            }
        }

        public void SummarizeUncertainty(
            ICollection<HbmIndividualDayCollection> collections,
            ICollection<Compound> substances,
            PopulationStratifier stratifier,
            double lowerBound,
            double upperBound
        ) {
            foreach (var substance in substances) {
                foreach (var collection in collections) {
                    var hbmConcentrations = getConcentrations(collection, substance, stratifier);
                    var timePointGroups = hbmConcentrations.GroupBy(r => r.Descriptor);
                    foreach (var group in timePointGroups) {
                        UpdateRecord(
                            hbmConcentrations,
                            group.Key,
                            collection.Target,
                            null,
                            lowerBound,
                            upperBound
                        );
                        if (stratifier != null) {
                            UpdateStratifiedRecords(
                                hbmConcentrations,
                                group.Key,
                                collection.Target,
                                lowerBound,
                                upperBound
                            );
                        }
                    }
                }
            }
        }

        private static List<HbmConcentrationsByDescriptor<HbmSubstanceTimePointContributorKey>> getConcentrations(
            HbmIndividualDayCollection collection,
            Compound substance,
            PopulationStratifier stratifier
        ) {
            var result = new List<HbmConcentrationsByDescriptor<HbmSubstanceTimePointContributorKey>>();
            var individualDaysByTimePoint = collection.HbmIndividualDayConcentrations
                .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                .GroupBy(r => r.TimePoint);
            foreach (var group in individualDaysByTimePoint) {
                var descriptor = new HbmSubstanceTimePointContributorKey() {
                    Substance = substance,
                    TimePoint = group.Key
                };
                result.AddRange(group.Select(c => new HbmConcentrationsByDescriptor<HbmSubstanceTimePointContributorKey> {
                    SamplingWeight = c.SimulatedIndividual.SamplingWeight,
                    TotalEndpointExposure = c.AverageEndpointSubstanceExposure(substance),
                    SourceSamplingMethods = c.ConcentrationsBySubstance
                        .TryGetValue(substance, out var record) ? record.SourceSamplingMethods : null,
                    StratificationLevel = stratifier?.GetLevel(c.SimulatedIndividual),
                    Descriptor = descriptor
                }));
            }
            return result;
        }
    }
}
