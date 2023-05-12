using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmSamplesBySamplingMethodSubstanceSection : SummarySection {

        public List<HbmSamplesBySamplingMethodSubstanceRecord> Records { get; set; }

        public List<HbmSampleConcentrationPercentilesRecord> HbmPercentilesRecords { get; set; }
        public List<HbmSampleConcentrationPercentilesRecord> HbmPercentilesAllRecords { get; set; }

        public void Summarize(
            ICollection<HumanMonitoringSampleSubstanceCollection> humanMonitoringSampleSubstanceCollection,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage
        ) {
            Records = summarizeHumanMonitoringSampleDetailsRecord(
                humanMonitoringSampleSubstanceCollection, 
                substances,
                lowerPercentage, 
                upperPercentage
            );
            summarizeHbmSampleBoxPlotRecord(humanMonitoringSampleSubstanceCollection, substances);
        }

        private static List<HbmSamplesBySamplingMethodSubstanceRecord> summarizeHumanMonitoringSampleDetailsRecord(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };

            var records = new List<HbmSamplesBySamplingMethodSubstanceRecord>();

            foreach (var samplingMethodGroup in hbmSampleSubstanceCollections) {
                var samples = samplingMethodGroup.HumanMonitoringSampleSubstanceRecords.Count();
                foreach (var substance in substances) {
                    var analysedSamples = samplingMethodGroup.HumanMonitoringSampleSubstanceRecords
                        .Where(r => r.HumanMonitoringSampleSubstances.ContainsKey(substance))
                        .ToList();

                    var sampleSubstances = analysedSamples
                        .Select(r => r.HumanMonitoringSampleSubstances[substance])
                        .ToList();
                    var positives = sampleSubstances.Where(r => r.IsPositiveResidue).ToList();

                    var percentilesSampleConcentrations = positives.Any() 
                        ? positives.Select(c => c.Residue).Percentiles(percentages)
                        : percentages.Select(r => double.NaN).ToArray();

                    var record = new HbmSamplesBySamplingMethodSubstanceRecord() {
                        SamplingType = samplingMethodGroup.SamplingMethod.SampleTypeCode,
                        BiologicalMatrix = samplingMethodGroup.SamplingMethod.BiologicalMatrix.GetDisplayName(),
                        ExposureRoute = samplingMethodGroup.SamplingMethod.ExposureRoute,
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        NumberOfSamples = samplingMethodGroup.HumanMonitoringSampleSubstanceRecords.Count(),
                        MeanPositives = positives.Any() ? positives.Average(c => c.Residue) : double.NaN,
                        LowerPercentilePositives = percentilesSampleConcentrations[0],
                        MedianPositives = percentilesSampleConcentrations[1],
                        UpperPercentilePositives = percentilesSampleConcentrations[2],
                        CensoredValuesMeasurements = sampleSubstances.Count(c => c.IsCensoredValue),
                        NonDetects = sampleSubstances.Count(c => c.IsNonDetect),
                        NonQuantifications = sampleSubstances.Count(c => c.IsNonQuantification),
                        PositiveMeasurements = sampleSubstances.Count(c => c.IsPositiveResidue),
                        MissingValueMeasurements = sampleSubstances.Count(c => c.IsMissingValue),
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

            HbmPercentilesRecords = new List<HbmSampleConcentrationPercentilesRecord>();
            HbmPercentilesAllRecords = new List<HbmSampleConcentrationPercentilesRecord>();
            foreach (var samplingMethodGroup in hbmSampleSubstanceCollections) {
                foreach (var substance in substances) {
                    var sampleSubstanceRecords = samplingMethodGroup.HumanMonitoringSampleSubstanceRecords
                        .SelectMany(r => r.HumanMonitoringSampleSubstances.Values.Where(c => c.MeasuredSubstance == substance))
                        .ToList();
                    var result = samplingMethodGroup.HumanMonitoringSampleSubstanceRecords
                        .Select(g => {
                            var sampleCompounds = g.HumanMonitoringSampleSubstances.Values
                                .Where(c => c.MeasuredSubstance == substance)
                                .ToList();
                            var positives = sampleCompounds.Where(c => c.IsPositiveResidue);
                            return (
                                Positive: sampleCompounds.Any(c => c.IsPositiveResidue),
                                Missing: sampleCompounds.All(c => c.IsMissingValue),
                                Residue: positives.Any() ? positives.Average(c => c.Residue) : double.NaN
                            );
                        })
                        .ToList();

                    var positiveConcentrations = result.Where(r => r.Positive).Select(r => r.Residue).ToList();
                    var percentiles = positiveConcentrations.Any() ? positiveConcentrations.Percentiles(percentages) : percentages.Select(r => double.NaN).ToArray();
                    var lod = sampleSubstanceRecords.Select(r => r.Lod).Distinct().Where(r => !double.IsNaN(r)).ToList();
                    var loq = sampleSubstanceRecords.Select(r => r.Loq).Distinct().Where(r => !double.IsNaN(r)).ToList();
                    var lor = sampleSubstanceRecords.Select(r => r.Lor).Distinct().Where(r => !double.IsNaN(r)).ToList();
                    var record = new HbmSampleConcentrationPercentilesRecord() {
                        MinPositives = positiveConcentrations.Any() ? positiveConcentrations.Min() : 0,
                        MaxPositives = positiveConcentrations.Any() ? positiveConcentrations.Max() : 0,
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        BiologicalMatrix = samplingMethodGroup.SamplingMethod.BiologicalMatrix.GetDisplayName(),
                        SampleTypeCode = samplingMethodGroup.SamplingMethod.SampleTypeCode,
                        LOR = lor.Any() ? lor.Max() : double.NaN,
                        Percentiles = percentiles.ToList(),
                        NumberOfMeasurements = result.Where(r => !r.Missing).Count(),
                        NumberOfPositives = positiveConcentrations.Count,
                        Percentage = positiveConcentrations.Count * 100d / result.Count()
                    };
                    if (record.NumberOfMeasurements > 0) {
                        HbmPercentilesRecords.Add(record);
                    }

                    var allConcentrations = result
                        .Where(r => !r.Missing)
                        .Select(r => r.Positive ? r.Residue : 0)
                        .ToList();
                    var results = allConcentrations.Any()
                        ? allConcentrations.Percentiles(percentages).ToList()
                        : percentages.Select(r => double.NaN).ToList();

                    var percentilesFull = results.Select(c => c == 0 ? double.NaN : c).ToList();
                    var recordFull = new HbmSampleConcentrationPercentilesRecord() {
                        MinPositives = positiveConcentrations.Any() ? positiveConcentrations.Min() : 0,
                        MaxPositives = positiveConcentrations.Any() ? positiveConcentrations.Max() : 0,
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        BiologicalMatrix = samplingMethodGroup.SamplingMethod.BiologicalMatrix.GetShortDisplayName(),
                        SampleTypeCode = samplingMethodGroup.SamplingMethod.SampleTypeCode,
                        LOR = lor.Any() ? lor.Max() : double.NaN,
                        Percentiles = percentilesFull.ToList(),
                        NumberOfPositives = positiveConcentrations.Count,
                        NumberOfMeasurements = result.Where(r => !r.Missing).Count(),
                        Percentage = positiveConcentrations.Count * 100d / result.Count()
                    };
                    if (recordFull.NumberOfMeasurements > 0) {
                        HbmPercentilesAllRecords.Add(recordFull);
                    }
                }
            }
        }
    }
}
