using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class PbkModelTimeCourseSection : SummarySection {

        public PbkModelSummaryRecord PbkModelSummaryRecord { get; set; }

        public List<PbkModelTimeCourseDrilldownRecord> InternalTargetSystemExposures { get; set; }
        public bool IsLifetime { get; set; }
        public int NumberOfDaysSkipped { get; set; }
        public double Maximum { get; set; }

        public void Summarize<T>(
            KineticModelInstance kineticModelInstance,
            ICollection<T> targetExposures,
            ICollection<ExposureRoute> routes,
            Compound substance,
            ICollection<TargetUnit> targetUnits,
            ExposureUnitTriple externalExposureUnit,
            bool isLifetime,
            int nonStationaryPeriod
       ) where T : AggregateIndividualExposureBase {
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
            
            IsLifetime = isLifetime;
            NumberOfDaysSkipped = nonStationaryPeriod;
            InternalTargetSystemExposures = results;
            Maximum = InternalTargetSystemExposures.Max(c => c.MaximumTargetExposure);
        }

        private List<PbkModelTimeCourseDrilldownRecord> getDrillDownSubstanceExposure(
            AggregateIndividualExposureBase aggregateExposure,
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
    }
}
