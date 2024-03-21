using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class KineticModelTimeCourseSection : SummarySection {

        private static readonly int _specifiedTakeNumer = 9;
        public List<InternalExposuresPerIndividual> InternalTargetSystemExposures { get; set; }
        public List<PBKDrilldownRecord> DrilldownRecords { get; set; }
        public ExposureType ExposureType { get; set; }
        public string ModelCode { get; set; }
        public int NumberOfDaysSkipped { get; set; }
        public double Maximum { get; set; }
        public int StepLength { get; set; }

        public void SummarizeIndividualDrillDown(
            ICollection<ITargetExposure> targetExposures,
            ICollection<ExposurePathType> exposureRoutes,
            Compound substance,
            KineticModelInstance kineticModelInstance,
            TargetUnit targetExposureUnit,
            ExposureType exposureType
        ) {
            ExposureType = exposureType;
            ModelCode = kineticModelInstance.IdModelDefinition;
            NumberOfDaysSkipped = kineticModelInstance.NonStationaryPeriod >= kineticModelInstance.NumberOfDays
                ? 0 : kineticModelInstance.NonStationaryPeriod;

            InternalTargetSystemExposures = targetExposures
                .Select(targetExposure => getDrillDownSubstanceExposure(
                    targetExposure,
                    substance,
                    exposureRoutes,
                    exposureType
                    )
                ).ToList();

            Maximum = InternalTargetSystemExposures.Max(c => c.MaximumTargetExposure);
            if (kineticModelInstance.ResolutionType == TimeUnit.Hours) {
                StepLength = 60 / kineticModelInstance.KineticModelDefinition.EvaluationFrequency;
            } else {
                StepLength = 1 / kineticModelInstance.KineticModelDefinition.EvaluationFrequency;
            }
            DrilldownRecords = InternalTargetSystemExposures
                .Select(c => new PBKDrilldownRecord() {
                    IndividualCode = !string.IsNullOrEmpty(c.Code) && !c.Code.Equals("-1") ? c.Code : string.Empty,
                    BodyWeight = c.Weight,
                    RelativeCompartmentWeight = c.RelativeCompartmentWeight,
                    Dietary = c.ExposurePerRoute.TryGetValue(ExposurePathType.Dietary.GetShortDisplayName(), out var diet) ? diet : null,
                    Oral = c.ExposurePerRoute.TryGetValue(ExposurePathType.Oral.GetShortDisplayName(), out var oral) ? oral : null,
                    Dermal = c.ExposurePerRoute.TryGetValue(ExposurePathType.Dermal.GetShortDisplayName(), out var dermal) ? dermal : null,
                    Inhalation = c.ExposurePerRoute.TryGetValue(ExposurePathType.Inhalation.GetShortDisplayName(), out var inhalation) ? inhalation : null,
                    ExternalDailyExposureAmount = c.ExternalExposure,
                    PeakSubstanceAmount = c.PeakTargetExposure,
                    PeakExposureAmount = c.InternalPeakTargetConcentration,
                    RatioPeak = c.PeakAbsorptionFactor,
                    LongSubstanceAmount = c.SteadyStateTargetExposure,
                    LongExposureAmount = c.InternalLongTermTargetConcentration,
                    RatioLong = c.LongTermAbsorptionFactor,
                }).ToList();
        }

        private InternalExposuresPerIndividual getDrillDownSubstanceExposure(
            ITargetExposure targetExposure,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureType exposureType
        ) {
            var exposure = targetExposure as TargetIndividualExposure;
            var result = new InternalExposuresPerIndividual() {
                Weight = exposure.Individual.BodyWeight,
                Code = exposure.Individual.Code,
                Covariable = exposure.Individual.Covariable,
                Cofactor = exposure.Individual.Cofactor,
                TargetExposure = exposure.GetExposureForSubstance(substance),
            };

            var compoundTargetSystemExposurePattern = exposure.GetSubstanceTargetExposure(substance) as SubstanceTargetExposurePattern;
            if (compoundTargetSystemExposurePattern != null) {
                result.MaximumTargetExposure = compoundTargetSystemExposurePattern.MaximumTargetExposure;
                result.RelativeCompartmentWeight = compoundTargetSystemExposurePattern.RelativeCompartmentWeight;
                result.ExposurePerRoute = new Dictionary<string, double>();
                foreach (var route in exposureRoutes) {
                    result.ExposurePerRoute[route.ToString()] = 0;
                }
                if (exposureType == ExposureType.Acute) {
                    if (targetExposure is AggregateIndividualDayExposure) {
                        var totalExposure = 0d;
                        foreach (var route in exposureRoutes) {
                            var exposurePerRoute = (exposure as AggregateIndividualDayExposure).ExposuresPerRouteSubstance[route]
                                .Where(s => s.Compound == substance)
                                .Sum(s => s.Exposure);
                            result.ExposurePerRoute[route.ToString()] += exposurePerRoute;
                            totalExposure += exposurePerRoute;
                        }
                        result.ExternalExposure = totalExposure;
                    }
                } else {
                    if (targetExposure is AggregateIndividualExposure) {
                        result.ExternalExposure = (exposure as AggregateIndividualExposure)
                            .ExternalIndividualDayExposures
                            ?.Select(c => {
                                var externalExposureDays = (exposure as AggregateIndividualExposure).ExternalIndividualDayExposures.Count;
                                var totalExposure = 0d;
                                foreach (var route in exposureRoutes) {
                                    if (c.ExposuresPerRouteSubstance.ContainsKey(route)) {
                                        var exposurePerRoute = c.ExposuresPerRouteSubstance[route]
                                            .Where(s => s.Compound == substance)
                                            .Sum(s => s.Exposure) / externalExposureDays;
                                        result.ExposurePerRoute[route.ToString()] += exposurePerRoute;
                                        totalExposure += exposurePerRoute;
                                    }
                                }
                                return totalExposure;
                            }).Sum()
                            ?? 0;
                    }
                }
                result.TargetExposures = compoundTargetSystemExposurePattern.TargetExposuresPerTimeUnit
                    .Select(r => new TargetExposurePerTimeUnitRecord() {
                        Exposure = r.Exposure,
                        Time = r.Time
                    })
                    .ToList();
                result.PeakTargetExposure = compoundTargetSystemExposurePattern.PeakTargetExposure;
                result.SteadyStateTargetExposure = compoundTargetSystemExposurePattern.SteadyStateTargetExposure;
            }
            return result;
        }

        /// <summary>
        /// Select the nine individuals according to specified drilldown percentage
        /// </summary>
        /// <param name="targetExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="percentageForDrilldown"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public static ICollection<ITargetExposure> GetTargetExposures(
           ICollection<ITargetExposure> targetExposures,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           double percentageForDrilldown,
           bool isPerPerson
        ) {
            var exposures = targetExposures.Cast<ITargetIndividualExposure>().ToList();
            var intakes = exposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = exposures.Select(c => c.IndividualSamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, percentageForDrilldown);
            var referenceIndividualIndex = intakes.Count(c => c < weightedPercentileValue);

            var lowerExtremePerson = _specifiedTakeNumer - 1;
            if (percentageForDrilldown != 100) {
                lowerExtremePerson = BMath.Floor(_specifiedTakeNumer / 2);
            }

            var result = exposures
                .OrderBy(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(_specifiedTakeNumer)
                .Cast<ITargetExposure>()
                .ToList();

            return result;
        }
    }
}
