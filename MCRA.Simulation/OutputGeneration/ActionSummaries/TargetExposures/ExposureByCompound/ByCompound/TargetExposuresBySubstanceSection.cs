using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TargetExposuresBySubstanceSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public TargetLevelType TargetLevel { get; set; }

        public List<DistributionCompoundRecord> Records { get; set; }

        public List<SubstanceTargetExposurePercentilesRecord> SubstanceBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            List<string> indexOrder,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            TargetLevel = TargetLevelType.External;
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
        
        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            List<string> indexOrder,
            ExposureUnitTriple externalExposureUnit
        ) {
            TargetLevel = TargetLevelType.Internal;
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            SubstanceBoxPlotRecords = getPercentileRecords(
                    aggregateExposures,
                    kineticConversionFactors,
                    substances,
                    externalExposureUnit)
                .OrderBy(x => indexOrder.IndexOf(x.SubstanceCode))
                .ToList();
        }

        /// <summary>
        /// Calculate summary statistics for boxplots dietary exposures acute.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="substances"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        private List<SubstanceTargetExposurePercentilesRecord> getPercentileRecordsAcute(
           ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
           ICollection<Compound> substances,
           bool isPerPerson
       ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = dietaryIndividualDayIntakes
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            Exposure: c.GetSubstanceTotalExposure(substance)
                                / (isPerPerson ? 1 : c.Individual.BodyWeight)
                        ))
                        .ToList();
                    return calculateTargetExposurePercentiles(substance, exposures);
                })
                .ToList();
        }

        /// <summary>
        /// Calculate summary statistics for boxplots dietary exposures chronic.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="substances"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        private List<SubstanceTargetExposurePercentilesRecord> getPercentileRecordsChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = dietaryIndividualDayIntakes
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => (
                            SamplingWeight: c.First().IndividualSamplingWeight,
                            Exposure: c.Sum(s => s.GetSubstanceTotalExposure(substance))
                                / c.Count()
                                / (isPerPerson ? 1 : c.First().Individual.BodyWeight)
                            )
                        )
                        .ToList();
                    return calculateTargetExposurePercentiles(substance, exposures);
                })
                .ToList();
        }

        /// <summary>
        /// Calculate summary statistics for boxplots target exposures chronic.
        /// </summary>
        private List<SubstanceTargetExposurePercentilesRecord> getPercentileRecords(
          ICollection<AggregateIndividualExposure> aggregateExposures,
          IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
          ICollection<Compound> substances,
          ExposureUnitTriple externalExposureUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = aggregateExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            Exposure: c.GetTotalExternalExposureForSubstance(
                                substance,
                                kineticConversionFactors,
                                externalExposureUnit.IsPerUnit()
                            )
                        ))
                        .ToList();
                    return calculateTargetExposurePercentiles(substance, exposures);
                })
                .ToList();
        }

        private static SubstanceTargetExposurePercentilesRecord calculateTargetExposurePercentiles(
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
            return new SubstanceTargetExposurePercentilesRecord() {
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Description = "Modelled",
                Percentiles = percentiles,
                NumberOfPositives = weights.Count,
                Percentage = weights.Count * 100d / exposures.Count
            };
        }
    }
}
