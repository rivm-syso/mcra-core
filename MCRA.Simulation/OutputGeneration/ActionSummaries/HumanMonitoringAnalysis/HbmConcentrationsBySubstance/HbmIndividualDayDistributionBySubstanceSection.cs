using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Constants;
using MCRA.Simulation.OutputGeneration.ActionSummaries;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmIndividualDayDistributionBySubstanceSection : ConcentrationBySubstanceSectionBase {

        public void Summarize(
            ICollection<HbmIndividualDayCollection> individualDayCollections,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            bool skipPrivacySensitiveOutputs
        ) {
            if (skipPrivacySensitiveOutputs) {
                foreach (var collection in individualDayCollections) {
                    var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(collection.HbmIndividualDayConcentrations.Count);
                    if (_upperWhisker > maxUpperPercentile) {
                        RestrictedUpperPercentile = maxUpperPercentile;
                        break;
                    }
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            var concentrationsAvailable = individualDayCollections
                .SelectMany(c => c.HbmIndividualDayConcentrations)
                .SelectMany(c => c.ConcentrationsBySubstance)
                .Select(c => c.Value.Concentration)
                .Sum() > 0;
            if (concentrationsAvailable) {
                var percentages = new double[] { lowerPercentage, 50, upperPercentage };
                foreach (var collection in individualDayCollections) {
                    foreach (var substance in substances) {
                        var record = GetSummaryRecord(percentages, collection, substance);
                        IndividualDayRecords.Add(record);
                    }
                }
                summarizeBoxPlotsPerMatrix(
                    individualDayCollections,
                    substances
                );
            }
        }

        public void SummarizeUncertainty(
            ICollection<HbmIndividualDayCollection> individualDayCollections,
            ICollection<Compound> substances,
            double lowerBound,
            double upperBound
        ) {
            foreach (var collection in individualDayCollections) {
                foreach (var substance in substances) {
                    var medianAll = GetSummaryRecord(collection, substance);
                    var record = IndividualDayRecords
                        .SingleOrDefault(c => c.SubstanceCode == substance.Code && c.CodeTargetSurface == collection.Target.Code);
                    if (record != null) {
                        record.MedianAllUncertaintyValues.Add(medianAll);
                        record.LowerUncertaintyBound = lowerBound;
                        record.UpperUncertaintyBound = upperBound;
                    }
                }
            }
        }

        /// <summary>
        /// Acute summarizer
        /// </summary>
        protected static HbmIndividualDayDistributionBySubstanceRecord GetSummaryRecord(
            double[] percentages,
            HbmIndividualDayCollection collection,
            Compound substance
        ) {
            var hbmIndividualDayConcentrations = collection.HbmIndividualDayConcentrations
                .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                .Select(c => (
                    samplingWeight: c.Individual.SamplingWeight,
                    totalEndpointExposures: c.AverageEndpointSubstanceExposure(substance),
                    sourceSamplingMethods: c.ConcentrationsBySubstance
                        .TryGetValue(substance, out var record) ? record.SourceSamplingMethods : null
                ))
                .ToList();

            if (!hbmIndividualDayConcentrations.Any()) {
                return createMissingRecord(substance, collection.TargetUnit);
            }

            var sourceSamplingMethods = hbmIndividualDayConcentrations
                .SelectMany(c => c.sourceSamplingMethods)
                .GroupBy(c => c)
                .Select(c => $"{c.Key.Name} ({c.Count()})")
                .ToList();

            var weights = hbmIndividualDayConcentrations
                .Where(c => c.totalEndpointExposures > 0)
                .Select(c => c.samplingWeight)
                .ToList();
            var percentiles = hbmIndividualDayConcentrations
                .Where(c => c.totalEndpointExposures > 0)
                .Select(c => c.totalEndpointExposures)
                .PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = hbmIndividualDayConcentrations.Select(c => c.samplingWeight).ToList();
            var percentilesAll = hbmIndividualDayConcentrations
                .Select(c => c.totalEndpointExposures)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new HbmIndividualDayDistributionBySubstanceRecord {
                CodeTargetSurface = collection.TargetUnit.Target.Code,
                BiologicalMatrix = collection.TargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? collection.TargetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = collection.TargetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? collection.TargetUnit.ExposureRoute.GetDisplayName()
                    : null,
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Unit = collection.TargetUnit.GetShortDisplayName(),
                ExpressionType = collection.TargetUnit?.ExpressionType != ExpressionType.None ? collection.TargetUnit?.ExpressionType.GetDisplayName() : "",
                MeanAll = hbmIndividualDayConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weightsAll.Sum(),
                PercentagePositives = weights.Count / (double)collection.HbmIndividualDayConcentrations.Count * 100,
                MeanPositives = hbmIndividualDayConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weights.Sum(),
                LowerPercentilePositives = percentiles[0],
                Median = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                NumberOfDays = weights.Count,
                SourceSamplingMethods = string.Join(", ", sourceSamplingMethods),
                MedianAllUncertaintyValues = new List<double>()
            };
            return record;
        }

        /// <summary>
        /// Acute summarizer uncertainty
        /// </summary>
        protected static double GetSummaryRecord(
            HbmIndividualDayCollection collection,
            Compound substance
        ) {
            var hbmIndividualDayConcentrations = collection.HbmIndividualDayConcentrations
                .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                .Select(c => (
                    samplingWeight: c.Individual.SamplingWeight,
                    totalEndpointExposures: c.AverageEndpointSubstanceExposure(substance),
                    sourceSamplingMethods: c.ConcentrationsBySubstance
                        .TryGetValue(substance, out var record) ? record.SourceSamplingMethods : null
                ))
                .ToList();

            var sourceSamplingMethods = hbmIndividualDayConcentrations
                .SelectMany(c => c.sourceSamplingMethods)
                .GroupBy(c => c)
                .Select(c => $"{c.Key.Name} ({c.Count()})")
                .ToList();

            var weightsAll = hbmIndividualDayConcentrations.Select(c => c.samplingWeight).ToList();
            var medianAll = hbmIndividualDayConcentrations
                .Select(c => c.totalEndpointExposures)
                .PercentilesWithSamplingWeights(weightsAll, 50);
            return medianAll;
        }

        /// <summary>
        /// Acute boxplot summarizer
        /// </summary>
        protected List<HbmConcentrationsPercentilesRecord> SummarizeBoxPlot(
            ICollection<HbmIndividualDayConcentration> individualDayConcentrations,
            ICollection<Compound> selectedSubstances,
            TargetUnit targetUnit
        ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var multipleSamplingMethods = IndividualDayRecords.Select(c => c.SourceSamplingMethods).Distinct().Count() > 1;
            foreach (var substance in selectedSubstances) {
                var hbmIndividualDayConcentrations = individualDayConcentrations
                    .Select(c => (
                        samplingWeight: c.Individual.SamplingWeight,
                        totalEndpointExposures: c.AverageEndpointSubstanceExposure(substance),
                        sourceSamplingMethods: c.ConcentrationsBySubstance.TryGetValue(substance, out var record)
                            ? record.SourceSamplingMethods : null
                    ))
                    .ToList();
                getBoxPlotRecord(
                    result,
                    multipleSamplingMethods,
                    substance,
                    hbmIndividualDayConcentrations,
                    targetUnit
                );
            }
            return result;
        }

        private void summarizeBoxPlotsPerMatrix(
            ICollection<HbmIndividualDayCollection> individualDayCollections,
            ICollection<Compound> substances
        ) {
            foreach (var collection in individualDayCollections) {
                var concentrationsPercentilesRecords = SummarizeBoxPlot(collection.HbmIndividualDayConcentrations, substances, collection.TargetUnit);
                if (concentrationsPercentilesRecords.Count > 0) {
                    HbmBoxPlotRecords[collection.Target] = concentrationsPercentilesRecords;
                }
            }
        }

        private static HbmIndividualDayDistributionBySubstanceRecord createMissingRecord(
           Compound substance,
           TargetUnit targetUnit
        ) {
            return new HbmIndividualDayDistributionBySubstanceRecord {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                CodeTargetSurface = targetUnit.Target.Code,
                BiologicalMatrix = targetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? targetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = targetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? targetUnit.ExposureRoute.GetDisplayName()
                    : null,
                Unit = targetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                SourceSamplingMethods = null,
                MedianAllUncertaintyValues = null
            };
        }
    }
}
