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
                .SelectMany(targetExposure => getDrillDownSubstanceExposure(
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
                    Compartment = c.Compartment,
                    CompartmentWeight = c.CompartmentWeight,
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

        private List<InternalExposuresPerIndividual> getDrillDownSubstanceExposure(
            ITargetExposure targetExposure,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureType exposureType
        ) {
            var results = new List<InternalExposuresPerIndividual>();
            var exposure = targetExposure as TargetIndividualExposure;
            var substanceCompartmentTargetExposuresPattern = (SubstanceTargetExposurePattern)exposure.GetSubstanceTargetExposure(substance);

            var substanceCompartmentTargetExposuresPatterns = new List<SubstanceTargetExposurePattern>() { substanceCompartmentTargetExposuresPattern };
            foreach (var pattern in substanceCompartmentTargetExposuresPatterns) {
                var exposurePerIndividual = new InternalExposuresPerIndividual() {
                    Weight = exposure.Individual.BodyWeight,
                    Code = exposure.Individual.Code,
                    Covariable = exposure.Individual.Covariable,
                    Cofactor = exposure.Individual.Cofactor,
                };
                if (pattern != null) {
                    exposurePerIndividual.TargetExposure = pattern.SubstanceAmount;
                    exposurePerIndividual.MaximumTargetExposure = pattern.MaximumTargetExposure;
                    exposurePerIndividual.RelativeCompartmentWeight = pattern.CompartmentInfo.relativeCompartmentWeight;
                    exposurePerIndividual.Compartment = pattern.CompartmentInfo.compartment;
                    exposurePerIndividual.ExposurePerRoute = new Dictionary<string, double>();
                    foreach (var route in exposureRoutes) {
                        exposurePerIndividual.ExposurePerRoute[route.ToString()] = 0;
                    }
                    if (exposureType == ExposureType.Acute) {
                        if (targetExposure is AggregateIndividualDayExposure) {
                            var totalExposure = 0d;
                            foreach (var route in exposureRoutes) {
                                var exposurePerRoute = (exposure as AggregateIndividualDayExposure).ExposuresPerRouteSubstance[route]
                                    .Where(s => s.Compound == substance)
                                    .Sum(s => s.Amount);
                                exposurePerIndividual.ExposurePerRoute[route.ToString()] += exposurePerRoute;
                                totalExposure += exposurePerRoute;
                            }
                            exposurePerIndividual.ExternalExposure = totalExposure;
                        }
                    } else {
                        if (targetExposure is AggregateIndividualExposure) {
                            exposurePerIndividual.ExternalExposure = (exposure as AggregateIndividualExposure)
                                .ExternalIndividualDayExposures
                                ?.Select(c => {
                                    var externalExposureDays = (exposure as AggregateIndividualExposure).ExternalIndividualDayExposures.Count;
                                    var totalExposure = 0d;
                                    foreach (var route in exposureRoutes) {
                                        if (c.ExposuresPerRouteSubstance.ContainsKey(route)) {
                                            var exposurePerRoute = c.ExposuresPerRouteSubstance[route]
                                                .Where(s => s.Compound == substance)
                                                .Sum(s => s.Amount) / externalExposureDays;
                                            exposurePerIndividual.ExposurePerRoute[route.ToString()] += exposurePerRoute;
                                            totalExposure += exposurePerRoute;
                                        }
                                    }
                                    return totalExposure;
                                }).Sum()
                                ?? 0;
                        }
                    }
                    exposurePerIndividual.TargetExposures = pattern.TargetExposuresPerTimeUnit
                        .Select(r => new TargetExposurePerTimeUnitRecord() {
                            Exposure = r.Exposure,
                            Time = r.Time
                        })
                        .ToList();
                    exposurePerIndividual.PeakTargetExposure = pattern.PeakTargetExposure;
                    exposurePerIndividual.SteadyStateTargetExposure = pattern.SteadyStateTargetExposure;
                }
                results.Add(exposurePerIndividual);
            }
            return results;
        }

        /// <summary>
        /// Select the nine individual ids according to specified drilldown percentage.
        /// This is done for one compartment under the assumption that the order of individuals is not different between compartments
        /// </summary>
        /// <param name="targetExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="percentageForDrilldown"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        public static ICollection<int> GetTargetExposuresIds(
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
            var simulatedIndividualIds = exposures
                .OrderBy(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(_specifiedTakeNumer)
                .Select(c => c.SimulatedIndividualId)
                .ToList();
            return simulatedIndividualIds;
        }
    }
}
