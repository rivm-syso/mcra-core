﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryExposuresBySubstanceSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<DietaryExposureBySubstancePercentileRecord> SubstanceBoxPlotRecords { get; set; }

        public ExposureUnitTriple ExposureUnit { get; set; }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            List<string> indexOrder,
            ExposureType exposureType,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            ExposureUnit = externalExposureUnit;
            if (exposureType == ExposureType.Acute) {
                SubstanceBoxPlotRecords = getPercentileRecordsAcute(dietaryIndividualDayIntakes, substances, isPerPerson)
                    .OrderBy(x => indexOrder.IndexOf(x.SubstanceCode))
                    .ToList();
            } else {
                SubstanceBoxPlotRecords = getPercentileRecordsChronic(dietaryIndividualDayIntakes, substances, isPerPerson)
                    .OrderBy(x => indexOrder.IndexOf(x.SubstanceCode))
                    .ToList();
            }
        }


        /// <summary>
        /// Calculate summary statistics for boxplots dietary exposures acute.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="substances"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        private List<DietaryExposureBySubstancePercentileRecord> getPercentileRecordsAcute(
           ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
           ICollection<Compound> substances,
           bool isPerPerson
       ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = dietaryIndividualDayIntakes
                        .Select(c => (
                            SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                            Exposure: c.GetSubstanceTotalExposure(substance)
                                / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                        ))
                        .ToList();
                    return calculateTargetExposurePercentiles(substance, exposures);
                })
                .ToList();
        }

        /// <summary>
        /// Calculate summary statistics for boxplots dietary exposures chronic.
        /// </summary>
        private List<DietaryExposureBySubstancePercentileRecord> getPercentileRecordsChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = dietaryIndividualDayIntakes
                        .GroupBy(c => c.SimulatedIndividual.Id)
                        .Select(c => (
                            SamplingWeight: c.First().SimulatedIndividual.SamplingWeight,
                            Exposure: c.Sum(s => s.GetSubstanceTotalExposure(substance))
                                / c.Count()
                                / (isPerPerson ? 1 : c.First().SimulatedIndividual.BodyWeight)
                            )
                        )
                        .ToList();
                    return calculateTargetExposurePercentiles(substance, exposures);
                })
                .ToList();
        }

        private static DietaryExposureBySubstancePercentileRecord calculateTargetExposurePercentiles(
            Compound substance,
            List<(double SamplingWeight, double Exposure)> exposures
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var weights = exposures
                .Select(a => a.SamplingWeight)
                .ToList();
            var allAxposures = exposures
                .Select(a => a.Exposure)
                .ToList();
            var percentiles = allAxposures
                .PercentilesWithSamplingWeights(weights, percentages)
                .ToList();
            var positives = allAxposures.Where(r => r > 0).ToList();
            return new DietaryExposureBySubstancePercentileRecord() {
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Percentiles = percentiles,
                NumberOfPositives = weights.Count,
                Percentage = weights.Count * 100d / exposures.Count
            };
        }
    }
}
