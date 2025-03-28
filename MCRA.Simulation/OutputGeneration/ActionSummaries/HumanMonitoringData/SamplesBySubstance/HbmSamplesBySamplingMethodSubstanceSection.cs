﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Constants;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData.SamplesBySubstance;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmSamplesBySamplingMethodSubstanceSection : SummarySection {

        private readonly double _upperWisker = 95d;
        public List<HbmSamplesBySamplingMethodSubstanceRecord> Records { get; set; }
        public SerializableDictionary<HumanMonitoringSamplingMethod, List<HbmSampleConcentrationPercentilesRecord>> HbmPercentilesRecords { get; set; } = [];
        public SerializableDictionary<HumanMonitoringSamplingMethod, List<HbmSampleConcentrationPercentilesRecord>> HbmPercentilesAllRecords { get; set; } = [];
        public List<HbmSampleConcentrationOutlierRecord> OutlierRecords { get; set; } = [];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public bool HasLodLoqRange { get; set; }

        public void Summarize(
            ICollection<HumanMonitoringSample> allHbmSamples,
            ICollection<HumanMonitoringSampleSubstanceCollection> humanMonitoringSampleSubstanceCollection,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            Dictionary<(HumanMonitoringSamplingMethod method, Compound a), List<string>> nonAnalysedSamples,
            bool skipPrivacySensitiveOutputs
        ) {
            if (skipPrivacySensitiveOutputs) {
                foreach (var record in humanMonitoringSampleSubstanceCollection) {
                    var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(record.HumanMonitoringSampleSubstanceRecords.Count);
                    if (_upperWisker > maxUpperPercentile) {
                        RestrictedUpperPercentile = maxUpperPercentile;
                        break;
                    }
                }
            }

            ShowOutliers = !skipPrivacySensitiveOutputs;
            Records = summarizeHumanMonitoringSampleDetailsRecord(
                allHbmSamples,
                humanMonitoringSampleSubstanceCollection,
                substances,
                lowerPercentage,
                upperPercentage,
                nonAnalysedSamples
            );
            summarizeHbmSampleBoxPlotRecord(humanMonitoringSampleSubstanceCollection, substances);
        }

        private List<HbmSamplesBySamplingMethodSubstanceRecord> summarizeHumanMonitoringSampleDetailsRecord(
            ICollection<HumanMonitoringSample> allHbmSamples,
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            Dictionary<(HumanMonitoringSamplingMethod method, Compound a), List<string>> nonAnalysedSamples
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var records = new List<HbmSamplesBySamplingMethodSubstanceRecord>();
            foreach (var sampleSubstanceCollection in hbmSampleSubstanceCollections) {
                var samples = sampleSubstanceCollection.HumanMonitoringSampleSubstanceRecords.Count;
                foreach (var substance in substances) {
                    var analysedSamples = sampleSubstanceCollection.HumanMonitoringSampleSubstanceRecords
                        .Where(r => r.HumanMonitoringSampleSubstances.ContainsKey(substance))
                        .ToList();

                    var sampleSubstances = analysedSamples
                        .Select(r => r.HumanMonitoringSampleSubstances[substance])
                        .ToList();
                    var positives = sampleSubstances.Where(r => r.IsPositiveResidue).ToList();
                    var percentilesSampleConcentrations = positives.Any()
                        ? positives.Select(c => c.Residue).Percentiles(percentages)
                        : percentages.Select(r => double.NaN).ToArray();

                    nonAnalysedSamples.TryGetValue((sampleSubstanceCollection.SamplingMethod, substance), out List<string> nonAnalysed);
                    var nonAnalysedCount = nonAnalysed?.Count ?? 0;

                    var record = new HbmSamplesBySamplingMethodSubstanceRecord() {
                        SamplingType = sampleSubstanceCollection.SamplingMethod.SampleTypeCode,
                        BiologicalMatrix = sampleSubstanceCollection.SamplingMethod.BiologicalMatrix.GetDisplayName(),
                        Unit = sampleSubstanceCollection.ConcentrationUnit.GetShortDisplayName(),
                        ExposureRoute = sampleSubstanceCollection.SamplingMethod.ExposureRoute,
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        SamplesTotal = sampleSubstanceCollection.HumanMonitoringSampleSubstanceRecords.Count,
                        SamplesAnalysed = sampleSubstanceCollection.HumanMonitoringSampleSubstanceRecords.Count - nonAnalysedCount,
                        SamplesNonAnalysed = nonAnalysedCount,
                        Lod = ComposeMinMaxStringRep(sampleSubstances.Select(r => r.Lod)),
                        Loq = ComposeMinMaxStringRep(sampleSubstances.Select(r => r.Loq)),
                        MeanPositives = positives.Any() ? positives.Average(c => c.Residue) : double.NaN,
                        LowerPercentilePositives = percentilesSampleConcentrations[0],
                        MedianPositives = percentilesSampleConcentrations[1],
                        UpperPercentilePositives = percentilesSampleConcentrations[2],
                        CensoredValuesMeasurements = sampleSubstances.Count(c => c.IsCensoredValue),
                        NonDetects = sampleSubstances.Count(c => c.IsNonDetect),
                        NonQuantifications = sampleSubstances.Count(c => c.IsNonQuantification),
                        PositiveMeasurements = sampleSubstances.Count(c => c.IsPositiveResidue),
                        MissingValueMeasurementsAnalysed = sampleSubstances.Count(c => c.IsMissingValue) - nonAnalysedCount,
                        MissingValueMeasurementsTotal = sampleSubstances.Count(c => c.IsMissingValue),
                        NumberOfIndividualDaysWithPositives = analysedSamples
                            .Where(r => r.HumanMonitoringSampleSubstances[substance].IsPositiveResidue)
                            .GroupBy(r => (r.Individual, r.Day))
                            .Count(),
                        NumberOfIndividualsWithPositives = analysedSamples
                            .Where(r => r.HumanMonitoringSampleSubstances[substance].IsPositiveResidue)
                            .GroupBy(r => r.Individual)
                            .Count(),
                    };
                    records.Add(record);
                }
            }
            return records;
        }

        private void summarizeHbmSampleBoxPlotRecord(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<Compound> substances
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };

            foreach (var collection in hbmSampleSubstanceCollections) {
                var hbmPercentilesRecords = new List<HbmSampleConcentrationPercentilesRecord>();
                var hbmPercentilesAllRecords = new List<HbmSampleConcentrationPercentilesRecord>();
                foreach (var substance in substances) {
                    var sampleSubstanceRecords = collection.HumanMonitoringSampleSubstanceRecords
                        .SelectMany(r => r.HumanMonitoringSampleSubstances.Values.Where(c => c.MeasuredSubstance == substance))
                        .ToList();
                    var result = collection.HumanMonitoringSampleSubstanceRecords
                        .Select(g => {
                            var sampleCompounds = g.HumanMonitoringSampleSubstances.Values
                                .Where(c => c.MeasuredSubstance == substance)
                                .ToList();
                            var positives = sampleCompounds.Where(c => c.IsPositiveResidue);
                            var samples = sampleCompounds.Any() ? sampleCompounds.Select(c => (c.MeasuredSubstance, c.Residue, indCode: g.Individual.Code, sampCode: g.HumanMonitoringSample.Code)).ToList() : null;
                            return (
                                Positive: sampleCompounds.Any(c => c.IsPositiveResidue),
                                Missing: sampleCompounds.All(c => c.IsMissingValue),
                                Residue: positives.Any() ? positives.Average(c => c.Residue) : double.NaN,
                                Samples: samples
                            );
                        })
                        .ToList();

                    var positiveSamples = result.Where(r => r.Positive).Select(r => (r.Residue, r.Samples)).ToList();
                    var percentiles = positiveSamples.Any() ? positiveSamples
                        .Select(c => c.Residue)
                        .Percentiles(percentages).ToList() : percentages.Select(r => double.NaN).ToList();
                    var outliers = positiveSamples.Where(c => c.Residue > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c.Residue < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                        .Select(c => (c.Residue, c.Samples)).ToList();
                    var descriptions = outliers.Select(r => r.Samples).ToList();
                    if (outliers.Any()) {
                        foreach (var description in descriptions) {
                            foreach (var item in description) {
                                var outlierRecord = new HbmSampleConcentrationOutlierRecord() {
                                    SubstanceName = substance.Name,
                                    SubstanceCode = substance.Code,
                                    BiologicalMatrix = collection.BiologicalMatrix.GetDisplayName(),
                                    IndividualCode = item.indCode,
                                    SampleCode = item.sampCode,
                                    Residue = item.Residue,
                                };
                                OutlierRecords.Add(outlierRecord);
                            }
                        }
                    }

                    var lor = sampleSubstanceRecords.Select(r => r.Lor).Distinct().Where(r => !double.IsNaN(r)).ToList();
                    var record = new HbmSampleConcentrationPercentilesRecord() {
                        Unit = collection.ConcentrationUnit.GetShortDisplayName(),
                        MinPositives = positiveSamples.Any() ? positiveSamples.Min(c => c.Residue) : 0,
                        MaxPositives = positiveSamples.Any() ? positiveSamples.Max(c => c.Residue) : 0,
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        BiologicalMatrix = collection.SamplingMethod.BiologicalMatrix.GetDisplayName(),
                        SampleTypeCode = collection.SamplingMethod.SampleTypeCode,
                        LOR = lor.Any() ? lor.Max() : double.NaN,
                        Percentiles = percentiles,
                        NumberOfMeasurements = result.Count(r => !r.Missing),
                        NumberOfPositives = positiveSamples.Count,
                        Percentage = positiveSamples.Count * 100d / result.Count,
                        Outliers = outliers.Select(c => c.Residue).ToList(),
                        NumberOfOutLiers = outliers.Count,
                    };
                    if (record.NumberOfMeasurements > 0) {
                        hbmPercentilesRecords.Add(record);
                    }

                    var allConcentrations = result
                        .Where(r => !r.Missing)
                        .Select(r => r.Positive ? r.Residue : 0)
                        .ToList();
                    var percentilesFull = allConcentrations.Any()
                        ? allConcentrations.Percentiles(percentages).ToList()
                        : percentages.Select(r => double.NaN).ToList();

                    var outliersFull = positiveSamples
                        .Where(c => c.Residue > percentilesFull[4] + 3 * (percentilesFull[4] - percentilesFull[2])
                            || c.Residue < percentilesFull[2] - 3 * (percentilesFull[4] - percentilesFull[2]))
                        .Select(c => c.Residue)
                        .ToList();

                    var recordFull = new HbmSampleConcentrationPercentilesRecord() {
                        Unit = collection.ConcentrationUnit.GetShortDisplayName(),
                        MinPositives = positiveSamples.Any() ? positiveSamples.Min(c => c.Residue) : 0,
                        MaxPositives = positiveSamples.Any() ? positiveSamples.Max(c => c.Residue) : 0,
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        BiologicalMatrix = collection.SamplingMethod.BiologicalMatrix.GetShortDisplayName(),
                        SampleTypeCode = collection.SamplingMethod.SampleTypeCode,
                        LOR = lor.Any() ? lor.Max() : double.NaN,
                        Percentiles = percentilesFull,
                        NumberOfPositives = positiveSamples.Count,
                        NumberOfMeasurements = result.Count(r => !r.Missing),
                        Percentage = positiveSamples.Count * 100d / result.Count,
                        Outliers = outliersFull,
                        NumberOfOutLiers = outliersFull.Count,
                    };
                    if (recordFull.NumberOfMeasurements > 0) {
                        hbmPercentilesAllRecords.Add(recordFull);
                    }
                }
                HbmPercentilesRecords[collection.SamplingMethod] = hbmPercentilesRecords;
                HbmPercentilesAllRecords[collection.SamplingMethod] = hbmPercentilesAllRecords;
            }
        }

        private string ComposeMinMaxStringRep(IEnumerable<double> doubles) {
            var hasValues = doubles.Any(d => !double.IsNaN(d));
            string strRep = string.Empty;
            if (hasValues) {
                var min = doubles.Where(d => !double.IsNaN(d)).Min();
                var max = doubles.Where(d => !double.IsNaN(d)).Max();
                if (min == max) {
                    strRep = $"{min:G3}";
                } else {
                    strRep = $"{min:G3}-{max:G3}";
                    HasLodLoqRange = true;
                }
            } else {
                strRep = "-";
            }
            return strRep;
        }
    }
}
