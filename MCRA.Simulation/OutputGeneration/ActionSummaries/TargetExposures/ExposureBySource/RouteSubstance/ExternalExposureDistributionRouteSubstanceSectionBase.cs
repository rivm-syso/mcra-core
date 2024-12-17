using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposureDistributionRouteSubstanceSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        protected double[] Percentages { get; set; }
        protected List<ExternalExposureDistributionRouteSubstanceRecord> SummarizeAcute(
                ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
                ICollection<Compound> selectedSubstances,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                ICollection<ExposureRoute> externalExposureRoutes,
                bool isPerPerson
            ) {
            var totalExternalExposure = externalIndividualDayExposures.Sum(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var exposuresCount = externalIndividualDayExposures.Count;
            var externalExposureDistributionRouteCompoundRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substances in selectedSubstances) {
                foreach (var route in externalExposureRoutes) {
                    var exposures = externalIndividualDayExposures
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            IntakePerMassUnit: c.GetTotalExposurePerRouteSubstance(route)
                                .Where(s => s.Compound == substances)
                                    .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substances], membershipProbabilities[substances])) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                        ))
                        .ToList();

                    var weights = exposures.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.SamplingWeight).ToList();
                    var percentiles = exposures.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, Percentages);

                    var rpf = relativePotencyFactors[substances];
                    var weightsAll = exposures.Select(c => c.SamplingWeight).ToList();
                    var percentilesAll = exposures
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);
                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substances.Name,
                        CompoundCode = substances.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalExternalExposure,
                        Percentage = weights.Count / (double)externalIndividualDayExposures.Count * 100,
                        Mean = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / weights.Sum() / rpf,
                        Percentile25 = percentiles[0] / rpf,
                        Median = percentiles[1] / rpf,
                        Percentile75 = percentiles[2] / rpf,
                        Percentile25All = percentilesAll[0] / rpf,
                        MedianAll = percentilesAll[1] / rpf,
                        Percentile75All = percentilesAll[2] / rpf,
                        RelativePotencyFactor = rpf,
                        NumberOfDays = weights.Count,
                        Contributions = []
                    };
                    externalExposureDistributionRouteCompoundRecords.Add(result);
                }
            }
            var rescale = externalExposureDistributionRouteCompoundRecords.Sum(c => c.Contribution);
            externalExposureDistributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            return externalExposureDistributionRouteCompoundRecords
                 .Where(r => r.Mean > 0)
                 .OrderBy(s => s.ExposureRoute, StringComparer.OrdinalIgnoreCase)
                 .ThenByDescending(r => r.Contribution)
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
            var totalExternalExposure = externalIndividualDayExposures
                .GroupBy(r => r.SimulatedIndividualId)
                .Sum(c => c.Sum(r => r.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().IndividualSamplingWeight / c.Count());
            var numberOfIndividuals = externalIndividualDayExposures.Select(c => c.SimulatedIndividualId).Distinct().Count();

            var externalExposureDistributionRouteCompoundRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in externalExposureRoutes) {

                    var externalExposureIndividualIntakes = externalIndividualDayExposures
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => (
                            c.First().Individual,
                            c.First().IndividualSamplingWeight,
                            IntakePerRouteCompound: c.First().GetTotalExposurePerRouteSubstance(route)
                        ))
                        .ToList();

                    var exposures = externalExposureIndividualIntakes.Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        IntakePerMassUnit: c.IntakePerRouteCompound
                            .Where(s => s.Compound == substance)
                            .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                    ))
                   .ToList();

                    var weights = exposures.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.SamplingWeight).ToList();

                    var percentiles = exposures.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, Percentages);

                    var rpf = relativePotencyFactors[substance];
                    var weightsAll = exposures.Select(c => c.SamplingWeight).ToList();
                    var percentilesAll = exposures
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);
                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substance.Name,
                        CompoundCode = substance.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalExternalExposure,
                        Percentage = weights.Count / (double)numberOfIndividuals * 100,
                        Mean = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / weights.Sum() / rpf,
                        Percentile25 = percentiles[0] / rpf,
                        Median = percentiles[1] / rpf,
                        Percentile75 = percentiles[2] / rpf,
                        Percentile25All = percentilesAll[0] / rpf,
                        MedianAll = percentilesAll[1] / rpf,
                        Percentile75All = percentilesAll[2] / rpf,
                        RelativePotencyFactor = rpf,
                        NumberOfDays = weights.Count,
                        Contributions = []
                    };
                    externalExposureDistributionRouteCompoundRecords.Add(result);
                }
            }
            var rescale = externalExposureDistributionRouteCompoundRecords.Sum(c => c.Contribution);
            externalExposureDistributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);

            return externalExposureDistributionRouteCompoundRecords
                 .Where(r => r.Mean > 0)
                 .OrderBy(s => s.ExposureRoute, StringComparer.OrdinalIgnoreCase)
                 .ThenByDescending(r => r.Contribution)
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
            var totalExternalExposure = externalIndividualDayExposures.Sum(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var intakesCount = externalIndividualDayExposures.Count;
            var externalExposureDistributionRouteSubstanceRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in externalExposureRoutes) {
                    var exposures = externalIndividualDayExposures
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            IntakePerMassUnit: c.GetTotalExposurePerRouteSubstance(route)
                                .Where(s => s.Compound == substance)
                                    .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                        ))
                        .ToList();

                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substance.Name,
                        CompoundCode = substance.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalExternalExposure,
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
                .GroupBy(r => r.SimulatedIndividualId)
                .Sum(c => c.Sum(r => r.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().IndividualSamplingWeight / c.Count());
            var numberOfIndividuals = externalIndividualDayExposures.Select(c => c.SimulatedIndividualId).Distinct().Count();

            var externalExposureDistributionRouteSubstanceRecords = new List<ExternalExposureDistributionRouteSubstanceRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in externalExposureExposureRoutes) {
                    var externalExposureIndividualIntakes = externalIndividualDayExposures
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => (
                            c.First().Individual,
                            c.First().IndividualSamplingWeight,
                            IntakePerRouteCompound: c.First().GetTotalExposurePerRouteSubstance(route)
                        ))
                        .ToList();

                    var exposures = externalExposureIndividualIntakes.Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        IntakePerMassUnit: c.IntakePerRouteCompound
                            .Where(s => s.Compound == substance)
                            .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                    ))
                   .ToList();

                    var result = new ExternalExposureDistributionRouteSubstanceRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substance.Name,
                        CompoundCode = substance.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalExternalExpousre,
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
