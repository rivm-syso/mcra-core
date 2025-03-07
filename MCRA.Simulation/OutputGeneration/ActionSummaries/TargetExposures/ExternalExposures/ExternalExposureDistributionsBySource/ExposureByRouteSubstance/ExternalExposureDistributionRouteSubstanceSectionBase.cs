using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposureDistributionRouteSubstanceSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        protected double[] Percentages { get; set; }

        protected List<ExternalExposureDistributionRouteSubstanceRecord> SummarizeAcute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            bool isPerPerson
        ) {
            var totalExternalExposure = relativePotencyFactors != null
                ? externalIndividualDayExposures
                    .Sum(c => c.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)
                        * c.SimulatedIndividual.SamplingWeight)
                : double.NaN;

            var externalExposureDistributionRouteSubstanceRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substance in substances) {
                var rpf = relativePotencyFactors?[substance] ?? double.NaN;
                foreach (var route in externalExposureRoutes) {
                    var exposures = externalIndividualDayExposures
                        .Select(c => (
                            c.SimulatedIndividual.SamplingWeight,
                            ExposurePerRouteSubstance: c.GetExposure(route, substance, isPerPerson)
                        ))
                        .ToList();

                    var weightsPositives = exposures.Where(c => c.ExposurePerRouteSubstance > 0)
                        .Select(c => c.SamplingWeight).ToList();
                    var percentilesPositives = exposures.Where(c => c.ExposurePerRouteSubstance > 0)
                        .Select(c => c.ExposurePerRouteSubstance)
                        .PercentilesWithSamplingWeights(weightsPositives, Percentages);

                    var weightsAll = exposures
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentilesAll = exposures
                        .Select(c => c.ExposurePerRouteSubstance)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);
                    var total = exposures.Sum(c => c.ExposurePerRouteSubstance * c.SamplingWeight);

                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        SubstanceName = substance.Name,
                        SubstanceCode = substance.Code,
                        Contribution = exposures.Sum(c => c.ExposurePerRouteSubstance * c.SamplingWeight * rpf) / totalExternalExposure,
                        PercentagePositives = weightsPositives.Count / (double)externalIndividualDayExposures.Count * 100,
                        MeanAll = total / weightsAll.Sum(),
                        MeanPositives = total / weightsPositives.Sum(),
                        Percentile25Positives = percentilesPositives[0],
                        MedianPositives = percentilesPositives[1],
                        Percentile75Positives = percentilesPositives[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        RelativePotencyFactor = rpf,
                        NumberOfIndividuals = weightsPositives.Count,
                        Contributions = []
                    };
                    externalExposureDistributionRouteSubstanceRecords.Add(result);
                }
            }
            var rescale = externalExposureDistributionRouteSubstanceRecords.Sum(c => c.Contribution);
            externalExposureDistributionRouteSubstanceRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            return externalExposureDistributionRouteSubstanceRecords
                .Where(r => r.MeanPositives > 0)
                .OrderByDescending(r => r.Contribution)
                .ThenBy(s => s.ExposureRoute)
                .ThenBy(s => s.SubstanceName)
                .ToList();
        }

        protected List<ExternalExposureDistributionRouteSubstanceRecord> SummarizeChronic(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            bool isPerPerson
        ) {
            var totalExternalExposure = relativePotencyFactors != null
                ? externalIndividualDayExposures
                    .GroupBy(r => r.SimulatedIndividual.Id)
                    .Sum(c => c.Sum(r => r.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        * c.First().SimulatedIndividual.SamplingWeight / c.Count())
                : double.NaN;
            var numberOfIndividuals = externalIndividualDayExposures.Select(c => c.SimulatedIndividual.Id).Distinct().Count();

            var externalExposureDistributionRouteSubstanceRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substance in selectedSubstances) {
                var rpf = relativePotencyFactors?[substance] ?? double.NaN;
                foreach (var route in externalExposureRoutes) {
                    var exposures = externalIndividualDayExposures
                        .Select(c => (
                            c.SimulatedIndividual,
                            c.SimulatedIndividual.SamplingWeight,
                            ExposurePerRouteSubstance: c.GetExposure(route, substance, isPerPerson)
                        ))
                        .ToList();

                    var weightsPositives = exposures
                        .Where(c => c.ExposurePerRouteSubstance > 0)
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var percentilesPositives = exposures
                        .Where(c => c.ExposurePerRouteSubstance > 0)
                        .Select(c => c.ExposurePerRouteSubstance)
                        .PercentilesWithSamplingWeights(weightsPositives, Percentages);

                    var weightsAll = exposures
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var percentilesAll = exposures
                        .Select(c => c.ExposurePerRouteSubstance)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);

                    var total = exposures.Sum(c => c.ExposurePerRouteSubstance * c.SamplingWeight);

                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        SubstanceName = substance.Name,
                        SubstanceCode = substance.Code,
                        Contribution = exposures.Sum(c => c.ExposurePerRouteSubstance * c.SamplingWeight * rpf) / totalExternalExposure,
                        PercentagePositives = weightsPositives.Count / (double)numberOfIndividuals * 100,
                        MeanAll = total / weightsAll.Sum(),
                        MeanPositives = total / weightsPositives.Sum(),
                        Percentile25Positives = percentilesPositives[0],
                        MedianPositives = percentilesPositives[1],
                        Percentile75Positives = percentilesPositives[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        RelativePotencyFactor = rpf,
                        NumberOfIndividuals = weightsPositives.Count,
                        Contributions = []
                    };
                    externalExposureDistributionRouteSubstanceRecords.Add(result);
                }
            }
            var rescale = externalExposureDistributionRouteSubstanceRecords.Sum(c => c.Contribution);
            externalExposureDistributionRouteSubstanceRecords.ForEach(c => c.Contribution = c.Contribution / rescale);

            return externalExposureDistributionRouteSubstanceRecords
                .Where(r => r.MeanPositives > 0)
                .OrderByDescending(r => r.Contribution)
                .ThenBy(s => s.ExposureRoute)
                .ThenBy(s => s.SubstanceName)
                .ToList();
        }

        protected List<ExternalExposureDistributionRouteSubstanceRecord> SummarizeAcuteUncertainty(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            bool isPerPerson
        ) {
            var totalExternalExposure = externalIndividualDayExposures.Sum(c => c.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.SimulatedIndividual.SamplingWeight);
            var exposuresCount = externalIndividualDayExposures.Count;
            var externalExposureDistributionRouteSubstanceRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in externalExposureRoutes) {
                    var exposures = externalIndividualDayExposures
                        .Select(c => (
                            SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                            ExposurePerRouteSubstance: c.GetExposuresBySubstance(route)
                                .Where(s => s.Compound == substance)
                                    .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])) / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                        ))
                        .ToList();

                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        SubstanceName = substance.Name,
                        SubstanceCode = substance.Code,
                        Contribution = exposures.Sum(c => c.ExposurePerRouteSubstance * c.SamplingWeight) / totalExternalExposure,
                    };
                    externalExposureDistributionRouteSubstanceRecords.Add(result);
                }
            }
            var rescale = externalExposureDistributionRouteSubstanceRecords.Sum(c => c.Contribution);
            externalExposureDistributionRouteSubstanceRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. externalExposureDistributionRouteSubstanceRecords];
        }

        protected List<ExternalExposureDistributionRouteSubstanceRecord> SummarizeChronicUncertainty(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureExposureRoutes,
            bool isPerPerson
        ) {
            var totalExternalExpousre = externalIndividualDayExposures
                .GroupBy(r => r.SimulatedIndividual.Id)
                .Sum(c => c.Sum(r => r.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().SimulatedIndividual.SamplingWeight / c.Count());
            var numberOfIndividuals = externalIndividualDayExposures.Select(c => c.SimulatedIndividual.Id).Distinct().Count();

            var externalExposureDistributionRouteSubstanceRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in externalExposureExposureRoutes) {
                    var externalExposureIndividualExposures = externalIndividualDayExposures
                        .GroupBy(c => c.SimulatedIndividual.Id)
                        .Select(c => (
                            c.First().SimulatedIndividual,
                            c.First().SimulatedIndividual.SamplingWeight,
                            ExposurePerRouteSubstance: c.First().GetExposuresBySubstance(route)
                        ))
                        .ToList();

                    var exposures = externalExposureIndividualExposures.Select(c => (
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                        ExposurePerRouteSubstance: c.ExposurePerRouteSubstance
                            .Where(s => s.Compound == substance)
                            .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])) / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                    ))
                   .ToList();

                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        SubstanceName = substance.Name,
                        SubstanceCode = substance.Code,
                        Contribution = exposures.Sum(c => c.ExposurePerRouteSubstance * c.SamplingWeight) / totalExternalExpousre,
                    };
                    externalExposureDistributionRouteSubstanceRecords.Add(result);
                }
            }
            var rescale = externalExposureDistributionRouteSubstanceRecords.Sum(c => c.Contribution);
            externalExposureDistributionRouteSubstanceRecords
                .ForEach(c => c.Contribution = c.Contribution / rescale);

            return [.. externalExposureDistributionRouteSubstanceRecords];
        }
    }
}
