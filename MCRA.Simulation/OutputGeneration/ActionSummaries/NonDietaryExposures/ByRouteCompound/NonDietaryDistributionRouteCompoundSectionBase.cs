﻿using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class NonDietaryDistributionRouteCompoundSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        protected double[] Percentages {  get; set; }

        protected List<NonDietaryDistributionRouteCompoundRecord> SummarizeAcute(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> nonDietaryExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes.Sum(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.SimulatedIndividual.SamplingWeight);
            var intakesCount = nonDietaryIndividualDayIntakes.Count;
            var nonDietaryDistributionRouteCompoundRecords = new List<NonDietaryDistributionRouteCompoundRecord>();
            foreach (var substances in selectedSubstances) {
                foreach (var route in nonDietaryExposureRoutes) {
                    var exposures = nonDietaryIndividualDayIntakes
                        .Select(c => (
                            SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                            IntakePerMassUnit: c.GetTotalIntakesPerRouteSubstance()
                                .Where(s => s.Compound == substances && s.Route == route)
                                    .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substances], membershipProbabilities[substances])) / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
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
                    var result = new NonDietaryDistributionRouteCompoundRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substances.Name,
                        CompoundCode = substances.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                        Percentage = weights.Count / (double)nonDietaryIndividualDayIntakes.Count * 100,
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
                    nonDietaryDistributionRouteCompoundRecords.Add(result);
                }
            }
            var rescale = nonDietaryDistributionRouteCompoundRecords.Sum(c => c.Contribution);
            nonDietaryDistributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            return nonDietaryDistributionRouteCompoundRecords
                 .Where(r => r.Mean > 0)
                 .OrderBy(s => s.ExposureRoute, StringComparer.OrdinalIgnoreCase)
                 .ThenByDescending(r => r.Contribution)
                 .ToList();
        }

        protected List<NonDietaryDistributionRouteCompoundRecord> SummarizeChronic(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> nonDietaryExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes
                .GroupBy(r => r.SimulatedIndividual.Id)
                .Sum(c => c.Sum(r => r.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().SimulatedIndividual.SamplingWeight / c.Count());
            var numberOfIndividuals = nonDietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.Id).Distinct().Count();

            var nonDietaryIndividualIntakes = nonDietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividual.Id)
                .Select(c => (
                    Individual: c.First().SimulatedIndividual,
                    IndividualSamplingWeight: c.First().SimulatedIndividual.SamplingWeight,
                    IntakePerRouteCompound: c.First().GetTotalIntakesPerRouteSubstance()
                ))
                .ToList();

            var nonDietaryDistributionRouteCompoundRecords = new List<NonDietaryDistributionRouteCompoundRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in nonDietaryExposureRoutes) {
                    var exposures = nonDietaryIndividualIntakes.Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        IntakePerMassUnit: c.IntakePerRouteCompound
                            .Where(s => s.Compound == substance && s.Route == route)
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
                    var result = new NonDietaryDistributionRouteCompoundRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substance.Name,
                        CompoundCode = substance.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
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
                    nonDietaryDistributionRouteCompoundRecords.Add(result);
                }
            }
            var rescale = nonDietaryDistributionRouteCompoundRecords.Sum(c => c.Contribution);
            nonDietaryDistributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);

            return nonDietaryDistributionRouteCompoundRecords
                 .Where(r => r.Mean > 0)
                 .OrderBy(s => s.ExposureRoute, StringComparer.OrdinalIgnoreCase)
                 .ThenByDescending(r => r.Contribution)
                 .ToList();
        }

        protected List<NonDietaryDistributionRouteCompoundRecord> SummarizeAcuteUncertainty(
        ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
        ICollection<Compound> selectedSubstances,
        IDictionary<Compound, double> relativePotencyFactors,
        IDictionary<Compound, double> membershipProbabilities,
        ICollection<ExposureRoute> nonDietaryExposureRoutes,
        bool isPerPerson
    ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes.Sum(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.SimulatedIndividual.SamplingWeight);
            var intakesCount = nonDietaryIndividualDayIntakes.Count;
            var nonDietaryDistributionRouteCompoundRecords = new List<NonDietaryDistributionRouteCompoundRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in nonDietaryExposureRoutes) {
                    var exposures = nonDietaryIndividualDayIntakes
                        .Select(c => (
                            SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                            IntakePerMassUnit: c.GetTotalIntakesPerRouteSubstance()
                                .Where(s => s.Compound == substance && s.Route == route)
                                    .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])) / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                        ))
                        .ToList();

                    var result = new NonDietaryDistributionRouteCompoundRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substance.Name,
                        CompoundCode = substance.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                    };
                    nonDietaryDistributionRouteCompoundRecords.Add(result);
                }
            }
            var rescale = nonDietaryDistributionRouteCompoundRecords.Sum(c => c.Contribution);
            nonDietaryDistributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            return nonDietaryDistributionRouteCompoundRecords.ToList();
        }

        protected List<NonDietaryDistributionRouteCompoundRecord> SummarizeChronicUncertainty(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> nonDietaryExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes
                .GroupBy(r => r.SimulatedIndividual.Id)
                .Sum(c => c.Sum(r => r.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().SimulatedIndividual.SamplingWeight / c.Count());
            var numberOfIndividuals = nonDietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.Id).Distinct().Count();

            var nonDietaryIndividualIntakes = nonDietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividual.Id)
                .Select(c => (
                    Individual: c.First().SimulatedIndividual,
                    IndividualSamplingWeight: c.First().SimulatedIndividual.SamplingWeight,
                    IntakePerRouteCompound: c.First().GetTotalIntakesPerRouteSubstance()
                ))
                .ToList();

            var nonDietaryDistributionRouteCompoundRecords = new List<NonDietaryDistributionRouteCompoundRecord>();
            foreach (var substance in selectedSubstances) {
                foreach (var route in nonDietaryExposureRoutes) {
                    var exposures = nonDietaryIndividualIntakes.Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        IntakePerMassUnit: c.IntakePerRouteCompound
                            .Where(s => s.Compound == substance && s.Route == route)
                            .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance])) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                    ))
                   .ToList();

                    var result = new NonDietaryDistributionRouteCompoundRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        CompoundName = substance.Name,
                        CompoundCode = substance.Code,
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                    };
                    nonDietaryDistributionRouteCompoundRecords.Add(result);
                }
            }
            var rescale = nonDietaryDistributionRouteCompoundRecords.Sum(c => c.Contribution);
            nonDietaryDistributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            return nonDietaryDistributionRouteCompoundRecords.ToList();
        }
    }
}
