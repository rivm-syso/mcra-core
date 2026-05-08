using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDayDistributionsSection
        : HbmCumulativeDistributionBySubstanceSectionBase<HbmSubstanceContributorKey, HbmConcentrationBySubstanceRecord, HbmSubstancePercentilesRecord> {

        public void Summarize(
            HbmCumulativeIndividualDayCollection collection,
            PopulationStratifier stratifier,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs
        ) {
            ExposureType = ExposureType.Acute;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(collection.HbmCumulativeIndividualDayConcentrations.Count);
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
                var results = CreateStratifiedBoxPlotRecords(
                    concentrations,
                    descriptor,
                    collection.TargetUnit
                );
                StratifiedHbmBoxPlotRecords = (collection.TargetUnit.Target, results);
            }
        }

        public void SummarizeUncertainty(
            HbmCumulativeIndividualDayCollection collection,
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
            HbmCumulativeIndividualDayCollection collection,
            PopulationStratifier stratifier
        ) {
            var concentrations = collection.HbmCumulativeIndividualDayConcentrations
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