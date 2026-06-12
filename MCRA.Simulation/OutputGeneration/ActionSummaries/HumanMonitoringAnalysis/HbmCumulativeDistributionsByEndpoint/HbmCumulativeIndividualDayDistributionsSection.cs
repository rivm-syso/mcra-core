using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDayDistributionsSection
        : HbmConcentrationByDescriptorSectionBase<HbmSubstanceContributorKey, HbmConcentrationBySubstanceRecord, HbmSubstancePercentilesRecord> {

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

            HbmBoxPlotRecords[collection.TargetUnit.Target] = hbmBoxPlotRecords;
            StratifiedHbmBoxPlotRecords[collection.TargetUnit.Target] = stratifiedHbmBoxPlotRecords;
        }

        public void SummarizeUncertainty(
            HbmCumulativeIndividualDayCollection collection,
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

        private static List<HbmConcentrationsByDescriptor<HbmSubstanceContributorKey>> getConcentrations(
            HbmCumulativeIndividualDayCollection collection,
            PopulationStratifier stratifier
        ) {
            var concentrations = collection.IndividualDayConcentrations
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