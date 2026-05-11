using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmIndividualDistributionBySubstanceSection
        : HbmConcentrationByDescriptorSectionBase<HbmSubstanceContributorKey, HbmConcentrationBySubstanceRecord, HbmSubstancePercentilesRecord> {

        public void Summarize(
            ICollection<HbmIndividualCollection> collections,
            ICollection<Compound> substances,
            PopulationStratifier stratifier,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs,
            bool detailsSection = false
        ) {
            ExposureType = ExposureType.Chronic;
            DetailsSection = detailsSection;
            if (skipPrivacySensitiveOutputs) {
                foreach (var collection in collections) {
                    var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(collection.HbmIndividualConcentrations.Count);
                    if (_upperWhisker > maxUpperPercentile) {
                        RestrictedUpperPercentile = maxUpperPercentile;
                        break;
                    }
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            var concentrationsAvailable = collections
                .SelectMany(c => c.HbmIndividualConcentrations)
                .SelectMany(c => c.ConcentrationsBySubstance)
                .Sum(c => c.Value.Exposure) > 0;

            if (concentrationsAvailable) {
                var percentages = new double[] { lowerPercentage, 50, upperPercentage };
                foreach (var collection in collections) {
                    var stratifiedHbmBoxPlotRecords = new List<HbmSubstancePercentilesRecord>();
                    var hbmBoxPlotRecords = new List<HbmSubstancePercentilesRecord>();
                    foreach (var substance in substances) {
                        var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
                        var hbmIndividualConcentrations = getConcentrations(collection, substance, stratifier);

                        var record = CreateSummaryRecord(
                            hbmIndividualConcentrations,
                            descriptor,
                            collection.TargetUnit,
                            null,
                            percentages
                        );
                        Records.Add(record); 

                        if (stratifier != null) {
                            var stratifiedRecords = CreateStratifiedSummaryRecords(
                                hbmIndividualConcentrations,
                                descriptor,
                                collection.TargetUnit,
                                percentages,
                                true
                            );
                            Records.AddRange(stratifiedRecords);
                        }

                        var result = CreateBoxPlotRecord(
                            hbmIndividualConcentrations,
                            descriptor,
                            null,
                            collection.TargetUnit
                        );
                        hbmBoxPlotRecords.Add(result);

                        if (stratifier != null) {
                            var stratifiedResults = CreateStratifiedBoxPlotRecords(
                                hbmIndividualConcentrations,
                                descriptor,
                                collection.TargetUnit);
                            stratifiedHbmBoxPlotRecords.AddRange(stratifiedResults);
                        }
                    }

                    HbmBoxPlotRecords[collection.Target] = hbmBoxPlotRecords;
                    StratifiedHbmBoxPlotRecords[collection.Target] = stratifiedHbmBoxPlotRecords;
                }
            }
        }

        public void SummarizeUncertainty(
            ICollection<HbmIndividualCollection> collections,
            ICollection<Compound> substances,
            PopulationStratifier stratifier,
            double lowerBound,
            double upperBound
        ) {
            foreach (var substance in substances) {
                foreach (var collection in collections) {
                    var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
                    var hbmConcentrations = getConcentrations(collection, substance, stratifier);
                    UpdateRecord(
                        hbmConcentrations,
                        descriptor,
                        collection.Target,
                        null,
                        lowerBound,
                        upperBound
                    );
                    if (stratifier != null) {
                        UpdateStratifiedRecords(
                            hbmConcentrations,
                            descriptor,
                            collection.Target,
                            lowerBound,
                            upperBound
                        );
                    }
                }
            }
        }

        private static List<HbmConcentrationsByDescriptor<HbmSubstanceContributorKey>> getConcentrations(
            HbmIndividualCollection collection,
            Compound substance,
            PopulationStratifier stratifier
        ) {
            var concentrations = collection.HbmIndividualConcentrations
                .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                .Select(c => new HbmConcentrationsByDescriptor<HbmSubstanceContributorKey> {
                    SamplingWeight = c.SimulatedIndividual.SamplingWeight,
                    TotalEndpointExposure = c.ConcentrationsBySubstance[substance].Exposure,
                    SourceSamplingMethods = c.ConcentrationsBySubstance
                        .TryGetValue(substance, out var record) ? record.SourceSamplingMethods : null,
                    StratificationLevel = stratifier?.GetLevel(c.SimulatedIndividual)
                })
                .ToList();
            return concentrations;
        }
    }
}
