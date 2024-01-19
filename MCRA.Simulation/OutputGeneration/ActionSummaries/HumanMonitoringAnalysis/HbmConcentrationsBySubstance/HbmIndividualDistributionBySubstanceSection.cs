using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmIndividualDistributionBySubstanceSection : ConcentrationBySubstanceSectionBase {

        public void Summarize(
            ICollection<HbmIndividualCollection> individualCollections,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var collection in individualCollections) {
                foreach (var substance in substances) {
                    var record = GetSummaryRecord(percentages, collection, substance);
                    IndividualRecords.Add(record);
                }
            }
            IndividualRecords = IndividualRecords
                .Where(r => r.MeanPositives > 0)
                .ToList();
            summarizeBoxPlotsPerMatrix(
                individualCollections,
                substances
            );
        }

        private void summarizeBoxPlotsPerMatrix(
            ICollection<HbmIndividualCollection> individualCollections,
            ICollection<Compound> substances
        ) {
            foreach (var collection in individualCollections) {
                var concentrationsPercentilesRecords = SummarizeBoxPlot(collection.HbmIndividualConcentrations, substances, collection.TargetUnit);
                if (concentrationsPercentilesRecords.Any()) {
                    HbmBoxPlotRecords[collection.TargetUnit.Target] = concentrationsPercentilesRecords;
                }
            }
        }

        public void SummarizeUncertainty(
            ICollection<HbmIndividualCollection> individualCollections,
            ICollection<Compound> substances,
            double lowerBound,
            double upperBound
        ) {
            foreach (var collection in individualCollections) {
                foreach (var substance in substances) {
                    var medianAll = GetSummaryRecord(collection, substance);
                    var record = IndividualRecords
                        .SingleOrDefault(c => c.SubstanceCode == substance.Code && c.CodeTargetSurface == collection.TargetUnit.Target.Code);
                    if (record != null) {
                        record.MedianAllUncertaintyValues.Add(medianAll);
                        record.LowerUncertaintyBound = lowerBound;
                        record.UpperUncertaintyBound = upperBound;
                    }
                }
            }
        }

        /// <summary>
        /// Chronic summarizer
        /// </summary>
        /// <param name="percentages"></param>
        /// <param name="collection"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        protected static HbmIndividualDistributionBySubstanceRecord GetSummaryRecord(
            double[] percentages,
            HbmIndividualCollection collection,
            Compound substance
        ) {
            var hbmIndividualConcentrations = collection.HbmIndividualConcentrations
                .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                .Select(c => (
                    samplingWeight: c.Individual.SamplingWeight,
                    totalEndpointExposures: c.ConcentrationsBySubstance[substance].Concentration,
                    sourceSamplingMethods: c.ConcentrationsBySubstance.TryGetValue(substance, out var record)
                        ? record.SourceSamplingMethods : null
                ))
                .ToList();

            var sourceSamplingMethods = hbmIndividualConcentrations
                .SelectMany(c => c.sourceSamplingMethods)
                .GroupBy(c => c)
                .Select(c => $"{c.Key.Name} ({c.Count()})")
                .ToList();

            var weights = hbmIndividualConcentrations.Where(c => c.totalEndpointExposures > 0)
                .Select(c => c.samplingWeight).ToList();
            var percentiles = hbmIndividualConcentrations.Where(c => c.totalEndpointExposures > 0)
                .Select(c => c.totalEndpointExposures)
                .PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = hbmIndividualConcentrations.Select(c => c.samplingWeight).ToList();
            var percentilesAll = hbmIndividualConcentrations
                .Select(c => c.totalEndpointExposures)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new HbmIndividualDistributionBySubstanceRecord {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                CodeTargetSurface = collection.TargetUnit.Target.Code,
                BiologicalMatrix = collection.TargetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? collection.TargetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = collection.TargetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? collection.TargetUnit.ExposureRoute.GetDisplayName()
                    : null,
                Unit = collection.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                MeanAll = hbmIndividualConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weightsAll.Sum(),
                PercentagePositives = weights.Count / (double)collection.HbmIndividualConcentrations.Count * 100,
                MeanPositives = hbmIndividualConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weights.Sum(),
                LowerPercentilePositives = percentiles[0],
                MedianPositives = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                IndividualsWithPositiveConcentrations = weights.Count,
                SourceSamplingMethods = string.Join(", ", sourceSamplingMethods),
                MedianAllUncertaintyValues = new List<double>()
            };
            return record;
        }

        /// <summary>
        /// Chronic summarizer uncertainty
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="substance"></param>
        /// <returns></returns>
        protected static double GetSummaryRecord(
            HbmIndividualCollection collection,
            Compound substance
        ) {
            var hbmIndividualConcentrations = collection.HbmIndividualConcentrations
                .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                .Select(c => (
                    samplingWeight: c.Individual.SamplingWeight,
                    totalEndpointExposures: c.ConcentrationsBySubstance[substance].Concentration,
                    sourceSamplingMethods: c.ConcentrationsBySubstance.TryGetValue(substance, out var record)
                        ? record.SourceSamplingMethods : null
                ))
                .ToList();

            var sourceSamplingMethods = hbmIndividualConcentrations
                .SelectMany(c => c.sourceSamplingMethods)
                .GroupBy(c => c)
                .Select(c => $"{c.Key.Name} ({c.Count()})")
                .ToList();

            var weightsAll = hbmIndividualConcentrations.Select(c => c.samplingWeight).ToList();
            var medianAll = hbmIndividualConcentrations
                .Select(c => c.totalEndpointExposures)
                .PercentilesWithSamplingWeights(weightsAll, 50);
            return medianAll;
        }

        /// <summary>
        /// Chronic boxplot summarizer
        /// </summary>
        /// <param name="individualConcentrations"></param>
        /// <param name="selectedSubstances"></param>
        /// <returns></returns>
        protected List<HbmConcentrationsPercentilesRecord> SummarizeBoxPlot(
            ICollection<HbmIndividualConcentration> individualConcentrations,
            ICollection<Compound> selectedSubstances,
            TargetUnit targetUnit
        ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var multipleSamplingMethods = IndividualDayRecords.Select(c => c.SourceSamplingMethods).Distinct().Count() > 1;
            foreach (var substance in selectedSubstances) {
                var hbmIndividualConcentrations = individualConcentrations
                    .Select(c => {
                        if (c.ConcentrationsBySubstance.TryGetValue(substance, out var substanceTargetConcentration)) {
                            return (
                                samplingWeight: c.Individual.SamplingWeight,
                                totalEndpointExposures: substanceTargetConcentration.Concentration,
                                sourceSamplingMethods: substanceTargetConcentration.SourceSamplingMethods
                            );
                        } else {
                            return (c.Individual.SamplingWeight, 0D, null);
                        }
                    })
                    .ToList();
                getBoxPlotRecord(
                    result,
                    _percentages,
                    multipleSamplingMethods,
                    substance,
                    hbmIndividualConcentrations,
                    targetUnit
                );
            }
            return result;
        }
    }
}
