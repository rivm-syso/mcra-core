﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries {
    public class ConcentrationBySubstanceSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
        public List<HbmIndividualDayDistributionBySubstanceRecord> IndividualDayRecords { get; set; } = new();
        public List<HbmIndividualDistributionBySubstanceRecord> IndividualRecords { get; set; } = new();
        public SerializableDictionary<ExposureTarget, List<HbmConcentrationsPercentilesRecord>> HbmBoxPlotRecords { get; set; } = new();

        /// <summary>
        /// Get boxplot record
        /// </summary>
        /// <param name="result"></param>
        /// <param name="percentages"></param>
        /// <param name="multipleSamplingMethods"></param>
        /// <param name="substance"></param>
        /// <param name="hbmIndividualDayConcentrations"></param>
        protected static void getBoxPlotRecord(
            List<HbmConcentrationsPercentilesRecord> result,
            bool multipleSamplingMethods,
            Compound substance,
            List<(double samplingWeight, double totalEndpointExposures, List<HumanMonitoringSamplingMethod> sourceSamplingMethods)> hbmIndividualDayConcentrations,
            TargetUnit targetUnit
        ) {
            if (hbmIndividualDayConcentrations.Any(c => c.totalEndpointExposures > 0)) {
                var sourceSamplingMethods = hbmIndividualDayConcentrations
                    .SelectMany(c => c.sourceSamplingMethods)
                    .GroupBy(c => c)
                    .Select(c => $"{c.Key.Name}")
                    .ToList();
                var weights = hbmIndividualDayConcentrations
                    .Select(c => c.samplingWeight)
                    .ToList();
                var allExposures = hbmIndividualDayConcentrations
                    .Select(c => c.totalEndpointExposures)
                    .ToList();
                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weights, _percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();

                var p95Idx = _percentages.Length - 1;
                var substanceName = percentiles[p95Idx] > 0 ? substance.Name : $"{substance.Name} *";
                var record = new HbmConcentrationsPercentilesRecord() {
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    SubstanceCode = substance.Code,
                    SubstanceName = substanceName,
                    Description = multipleSamplingMethods ? $"{substanceName} {string.Join(", ", sourceSamplingMethods)}" : substanceName,
                    Percentiles = percentiles.ToList(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / hbmIndividualDayConcentrations.Count,
                    Unit = targetUnit.ExposureUnit.GetShortDisplayName()
                };
                result.Add(record);
            }
        }
    }
}
