using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsSection
        : HbmCumulativeDistributionBySubstanceSectionBase<HbmSubstanceContributorKey, HbmConcentrationBySubstanceRecord, HbmSubstancePercentilesRecord> {

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
                    percentages
                );
                Records.AddRange(stratifiedRecords);
            }

            var hbmBoxPlotRecord = CreateBoxPlotRecord(
                concentrations,
                null,
                descriptor,
                collection.TargetUnit
            );
            BoxPlotRecord = (collection.TargetUnit.Target, hbmBoxPlotRecord);

            if (stratifier != null) {
                var stratifiedConcentrations = getConcentrations(collection, stratifier);
                var results = CreateStratifiedBoxPlotRecords(
                    stratifiedConcentrations,
                    descriptor,
                    collection.TargetUnit
                );
                StratifiedHbmBoxPlotRecords = (collection.TargetUnit.Target, results);
            }
        }

        public void SummarizeUncertainty(
            HbmCumulativeIndividualCollection collection,
            PopulationStratifier stratifier,
            double lowerBound,
            double upperBound
        ) {
            var substance = new Compound() { Name = "Cumulative", Code = "Cumulative" };
            var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
            var unstratifiedConcentrations = getConcentrations(collection, null);
            UpdateRecord(
                unstratifiedConcentrations,
                descriptor,
                null,
                lowerBound,
                upperBound
            );
            if (stratifier != null) {
                var stratifiedConcentrations = getConcentrations(collection, stratifier);
                UpdateStratifiedRecords(
                    stratifiedConcentrations,
                    descriptor,
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
                    StratificationLevel = stratifier?.GetLevel(c.SimulatedIndividual)
                })
                .ToList();
            return concentrations;
        }
    }
}
