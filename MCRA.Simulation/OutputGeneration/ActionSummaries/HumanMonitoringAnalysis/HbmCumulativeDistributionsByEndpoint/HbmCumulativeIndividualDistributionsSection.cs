using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsSection
        : HbmConcentrationByDescriptorSectionBase<HbmSubstanceContributorKey, HbmConcentrationBySubstanceRecord, HbmSubstancePercentilesRecord> {

        public void Summarize(
            HbmCumulativeIndividualCollection collection,
            PopulationStratifier stratifier,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs
        ) {
            ExposureType = ExposureType.Chronic;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(collection.HbmCumulativeIndividualConcentrations.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var substance = new Compound() { Name = "Cumulative", Code = "Cumulative" };
            var descriptor = new HbmSubstanceContributorKey() { Substance = substance };

            var stratifiedHbmBoxPlotRecords = new List<HbmSubstancePercentilesRecord>();
            var hbmBoxPlotRecords = new List<HbmSubstancePercentilesRecord>();

            var concentrations = getConcentrations(collection, stratifier);
            var record = CreateSummaryRecord(
                concentrations,
                descriptor,
                collection.TargetUnit,
                null,
                percentages
            );
            Records.Add(record);

            if (stratifier != null) {
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

            if (stratifier != null) {
                var stratifiedResults = CreateStratifiedBoxPlotRecords(
                    concentrations,
                    descriptor,
                    collection.TargetUnit
                );
                stratifiedHbmBoxPlotRecords.AddRange(stratifiedResults);
            }

            HbmBoxPlotRecords[collection.TargetUnit.Target] = hbmBoxPlotRecords;
            StratifiedHbmBoxPlotRecords[collection.TargetUnit.Target] = stratifiedHbmBoxPlotRecords;
        }

        public void SummarizeUncertainty(
            HbmCumulativeIndividualCollection collection,
            PopulationStratifier stratifier,
            double lowerBound,
            double upperBound
        ) {
            var substance = new Compound() { Name = "Cumulative", Code = "Cumulative" };
            var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
            var hbmConcentrations = getConcentrations(collection, stratifier);
            UpdateRecord(
                hbmConcentrations,
                descriptor,
                collection.TargetUnit.Target,
                null,
                lowerBound,
                upperBound
            );
            if (stratifier != null) {
                UpdateStratifiedRecords(
                    hbmConcentrations,
                    descriptor,
                    collection.TargetUnit.Target,
                    lowerBound,
                    upperBound
                );
            }
        }

        private static List<HbmConcentrationsByDescriptor<HbmSubstanceContributorKey>> getConcentrations(
            HbmCumulativeIndividualCollection collection,
            PopulationStratifier stratifier
        ) {
            var concentrations = collection.HbmCumulativeIndividualConcentrations
                .Select(c => new HbmConcentrationsByDescriptor<HbmSubstanceContributorKey> {
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
