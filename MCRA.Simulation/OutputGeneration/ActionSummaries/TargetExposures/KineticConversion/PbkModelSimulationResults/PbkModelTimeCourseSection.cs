using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class PbkModelTimeCourseSection : SummarySection {

        public PbkModelSummaryRecord PbkModelSummaryRecord { get; set; }

        private static readonly int _specifiedTakeNumer = 9;
        public List<PbkModelTimeCourseDrilldownRecord> InternalTargetSystemExposures { get; set; }
        public int NumberOfDaysSkipped { get; set; }
        public double Maximum { get; set; }

        public void Summarize(
            KineticModelInstance kineticModelInstance,
           ICollection<AggregateIndividualExposure> targetExposures,
           ICollection<ExposureRoute> routes,
           Compound substance,
           ICollection<TargetUnit> targetUnits,
           ExposureUnitTriple externalExposureUnit,
           int nonStationaryPeriod
       ) {
            if (targetUnits.Count > 1) {
                throw new NotImplementedException();
            }

            var targetUnit = targetUnits.FirstOrDefault();
            PbkModelSummaryRecord = new PbkModelSummaryRecord() {
                ModelCode = kineticModelInstance.KineticModelDefinition.Id,
                ModelName = kineticModelInstance.KineticModelDefinition.Name,
                ModelInstanceCode = kineticModelInstance.IdModelInstance,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Routes = string.Join(", ", routes.Select(c => c.GetShortDisplayName())),
                ExposureTarget = targetUnit.Target.GetDisplayName()
            };

            var results = targetExposures
                .SelectMany(targetExposure =>
                    getDrillDownSubstanceExposure(
                        targetExposure,
                        substance,
                        routes,
                        targetUnits,
                        externalExposureUnit
                    )
                )
                .ToList();
            NumberOfDaysSkipped = nonStationaryPeriod;
            InternalTargetSystemExposures = results;
            Maximum = InternalTargetSystemExposures.Max(c => c.MaximumTargetExposure);
        }

        private List<PbkModelTimeCourseDrilldownRecord> getDrillDownSubstanceExposure(
            AggregateIndividualExposure aggregateExposure,
            Compound substance,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            ExposureUnitTriple externalExposureUnit
        ) {
            var results = new List<PbkModelTimeCourseDrilldownRecord>();

            var substanceCompartmentTargetExposuresPatterns = targetUnits
                .Select(r => aggregateExposure
                    .GetSubstanceTargetExposure(r.Target, substance) as SubstanceTargetExposurePattern
                )
                .Where(r => r != null)
                .ToList();
            foreach (var pattern in substanceCompartmentTargetExposuresPatterns) {
                var record = new PbkModelTimeCourseDrilldownRecord() {
                    BodyWeight = aggregateExposure.SimulatedIndividual.BodyWeight,
                    Age = aggregateExposure.SimulatedIndividual.Age ?? double.NaN,
                    IndividualCode = aggregateExposure.SimulatedIndividual.Code
                };
                if (pattern != null) {
                    var targetUnit = targetUnits.First();
                    record.TargetExposure = pattern.Exposure;
                    record.MaximumTargetExposure = pattern.MaximumTargetExposure;
                    record.RelativeCompartmentWeight = pattern.RelativeCompartmentWeight;
                    record.ExternalExposure = aggregateExposure
                        .GetTotalExternalExposureForSubstance(substance, externalExposureUnit.IsPerUnit);

                    var exposurePerRoute = routes
                        .ToDictionary(
                            route => route,
                            route => aggregateExposure
                                .GetTotalRouteExposureForSubstance(
                                    route,
                                    substance,
                                    externalExposureUnit.IsPerUnit
                                )
                        );
                    record.TargetExposures = pattern.TargetExposuresPerTimeUnit?
                        .Select(r => new TargetIndividualExposurePerTimeUnitRecord() {
                            Exposure = r.Exposure,
                            Time = r.Time
                        })
                        .ToList() ?? [];
                    record.Unit = targetUnit.GetShortDisplayName();
                    record.BiologicalMatrix = targetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                        ? targetUnit.BiologicalMatrix.GetDisplayName() : null;
                    record.ExpressionType = targetUnit.ExpressionType != ExpressionType.None
                        ? targetUnit.ExpressionType.GetDisplayName() : null;
                    record.RatioInternalExternal = record.TargetExposure / record.ExternalExposure;
                    record.Oral = exposurePerRoute.TryGetValue(ExposureRoute.Oral, out var oral) ? oral : null;
                    record.Dermal = exposurePerRoute.TryGetValue(ExposureRoute.Dermal, out var dermal) ? dermal : null;
                    record.Inhalation = exposurePerRoute.TryGetValue(ExposureRoute.Inhalation, out var inhalation) ? inhalation : null;
                }
                results.Add(record);
            }
            return results;
        }

        /// <summary>
        /// Select the nine individual ids according to specified drilldown percentage.
        /// This is done for one compartment under the assumption that the order of individuals is not different between compartments
        /// </summary>
        public static HashSet<int> GetDrilldownIndividualIds<T>(
           ICollection<T> exposures,
           ICollection<Compound> substances,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           double percentageForDrilldown,
           TargetUnit targetUnit
        ) where T : AggregateIndividualExposure {
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            var intakes = exposures
                .Select(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities
                ));
            var weights = exposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, percentageForDrilldown);
            var referenceIndividualIndex = intakes.Count(c => c < weightedPercentileValue);
            var lowerExtremePerson = _specifiedTakeNumer - 1;
            if (percentageForDrilldown != 100) {
                lowerExtremePerson = BMath.Floor(_specifiedTakeNumer / 2);
            }
            var simulatedIndividualIds = exposures
                .OrderBy(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities
                ))
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(_specifiedTakeNumer)
                .Select(c => c.SimulatedIndividual.Id)
                .ToHashSet();
            return simulatedIndividualIds;
        }
    }
}
