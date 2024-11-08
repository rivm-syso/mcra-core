using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryDrillDownSection : SummarySection {

        public bool IsCumulative { get; set; }
        public double VariabilityDrilldownPercentage { get; set; }
        public double PercentileValue { get; set; }
        public string ReferenceCompoundName { get; set; }
        public List<NonDietaryDrillDownRecord> DrillDownSummaryRecords { get; set; }

        public List<int> GetDrillDownRecords(
            double percentageForDrilldown,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            VariabilityDrilldownPercentage = percentageForDrilldown;

            var nonDietaryIndividualIntakes = nonDietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => c.First())
                .ToList();

            var referenceIndividualIndex = BMath.Floor(nonDietaryIndividualIntakes.Count * VariabilityDrilldownPercentage / 100);
            var intakes = nonDietaryIndividualIntakes.Select(c => c.TotalNonDietaryIntakePerMassUnit(kineticConversionFactors, relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = nonDietaryIndividualIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, VariabilityDrilldownPercentage);
            referenceIndividualIndex = nonDietaryIndividualIntakes
                .Where(c => c.TotalNonDietaryIntakePerMassUnit(kineticConversionFactors, relativePotencyFactors, membershipProbabilities, isPerPerson) < weightedPercentileValue)
                .Count();

            var specifiedTakeNumer = 9;
            var lowerExtremePerson = specifiedTakeNumer - 1;
            if (VariabilityDrilldownPercentage != 100) {
                lowerExtremePerson = BMath.Floor(specifiedTakeNumer / 2);
            }

            var ids = nonDietaryIndividualIntakes
                .OrderBy(c => c.TotalNonDietaryIntakePerMassUnit(kineticConversionFactors, relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(specifiedTakeNumer)
                .Select(c => c.SimulatedIndividualId)
                .ToList();

            return ids;
        }

        public void Summarize(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            ICollection<int> selectedIndividuals,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            Compound reference,
            bool isPerPerson
        ) {
            ReferenceCompoundName = reference.Name;
            IsCumulative = relativePotencyFactors.Count > 1;
            var nonDietaryIndividualIntakes = nonDietaryIndividualDayIntakes
               .GroupBy(c => c.SimulatedIndividualId)
               .Select(c => c.First())
               .ToList();

            var drillDownTargets = nonDietaryIndividualIntakes
                .OrderBy(c => c.TotalNonDietaryIntakePerMassUnit(kineticConversionFactors, relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Where(c => selectedIndividuals.Contains(c.SimulatedIndividualId))
                .ToList();

            var ix = BMath.Floor(drillDownTargets.Count / 2);

            PercentileValue = drillDownTargets.ElementAt(ix).TotalNonDietaryIntakePerMassUnit(kineticConversionFactors, relativePotencyFactors, membershipProbabilities, isPerPerson);
            DrillDownSummaryRecords = [];

            foreach (var item in drillDownTargets) {
                var bodyWeight = item.Individual.BodyWeight;
                var ndIpc = item?.NonDietaryIntake?.NonDietaryIntakesPerCompound ?? [];
                var nonDietaryIntakeSummaryPerCompoundRecords = ndIpc.GroupBy(ndipc => ndipc.Compound)
                    .Select(g => {

                        var exposurePerRoute = new Dictionary<ExposurePathType, double>();
                        foreach (var route in nonDietaryExposureRoutes) {
                            exposurePerRoute[route] = g.Where(r => r.Route == route).Sum(r => r.Amount) / bodyWeight;
                        }
                        return new NonDietaryIntakeSummaryPerCompoundRecord {
                            SubstanceCode = g.Key.Code,
                            SubstanceName = g.Key.Name,
                            UncorrectedRouteIntakeRecords = exposurePerRoute.Select(c => new RouteIntakeRecord() {
                                Route = c.Key.GetShortDisplayName(),
                                Exposure = c.Value,
                                AbsorptionFactor = kineticConversionFactors[(c.Key, g.Key)],
                            }).ToList(),
                            NonDietaryIntakeAmountPerBodyWeight = g.Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / bodyWeight,
                            NumberOfNondietaryContributions = g.Count(),
                            RelativePotencyFactor = relativePotencyFactors[g.Key],
                        };
                    })
                    .GroupBy(gr => gr.SubstanceCode)
                    .Select(c => {
                        var exposurePerRoute = new Dictionary<ExposurePathType, double>();
                        foreach (var route in nonDietaryExposureRoutes) {
                            exposurePerRoute[route] = c.SelectMany(s => s.UncorrectedRouteIntakeRecords).Where(r => r.Route == route.GetShortDisplayName()).Sum(s => s.Exposure) / bodyWeight;
                        }
                        var compound = relativePotencyFactors.FirstOrDefault(r => r.Key.Code == c.Key).Key;
                        return new NonDietaryIntakeSummaryPerCompoundRecord() {
                            SubstanceCode = c.Key,
                            SubstanceName = c.Last().SubstanceName,
                            UncorrectedRouteIntakeRecords = exposurePerRoute.Select(s => new RouteIntakeRecord() {
                                Route = s.Key.GetShortDisplayName(),
                                Exposure = s.Value,
                                AbsorptionFactor = kineticConversionFactors[(s.Key, compound)],
                            }).ToList(),
                            RelativePotencyFactor = c.Average(i => i.RelativePotencyFactor),
                            NonDietaryIntakeAmountPerBodyWeight = c.Sum(i => i.NonDietaryIntakeAmountPerBodyWeight),
                            NumberOfNondietaryContributions = c.Sum(i => i.NumberOfNondietaryContributions),
                        };
                    })
                    .ToList();

                var exposuresPerRoutePerBodyWeight = new Dictionary<ExposurePathType, double>();
                foreach (var route in nonDietaryExposureRoutes) {
                    exposuresPerRoutePerBodyWeight[route] = item.TotalNonDietaryExposurePerRoute(route, relativePotencyFactors, membershipProbabilities) / bodyWeight;
                }
                var nonDietaryIntakePerBodyWeight = item.TotalNonDietaryIntake(kineticConversionFactors, relativePotencyFactors, membershipProbabilities) / item.Individual.BodyWeight;
                var result = new NonDietaryDrillDownRecord() {
                    Guid = item.SimulatedIndividualDayId.ToString(),
                    IndividualCode = item.Individual.Code,
                    BodyWeight = bodyWeight,
                    SamplingWeight = item.IndividualSamplingWeight, // Indeed, here we print the original sampling weight!!!
                    Day = item.Day,
                    NonDietaryIntakePerBodyWeight = nonDietaryIntakePerBodyWeight,
                    CorrectedRouteIntakeRecords = exposuresPerRoutePerBodyWeight.Select(c => new RouteIntakeRecord() {
                        Route = c.Key.GetShortDisplayName(),
                        Exposure = c.Value,
                    }).ToList(),
                    NonDietaryIntakeSummaryPerCompoundRecords = nonDietaryIntakeSummaryPerCompoundRecords,
                };

                DrillDownSummaryRecords.Add(result);
            }
        }
    }
}
