using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class KineticModelTimeCourseSection : SummarySection {

        private static int _specifiedTakeNumer = 9;

        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public string ModelDescription { get; set; }
        public string OutputCode { get; set; }
        public string SubstanceCode { get; set; }
        public string SubstanceName { get; set; }
        public string OutputDescription { get; set; }
        public int NumberOfDosesPerDay { get; set; }
        public int NumberOfIndividuals { get; set; }
        public int NumberOfDays { get; set; }
        public int NumberOfDaysSkipped { get; set; }
        public string DoseUnit { get; set; }
        public string TimeUnit { get; set; }
        public List<string> ExposureRoutes { get; set; }
        public double Maximum { get; set; }
        public double ConcentrationRatioPeak { get; set; }
        public double ConcentrationRatioAverage { get; set; }
        public int StepLength { get; set; }
        public List<InternalExposuresPerIndividual> InternalTargetSystemExposures { get; set; }

        public bool IsAcute { get; set; }

        public void SummarizeIndividualDrillDown(
            ICollection<ITargetIndividualExposure> drillDownRecords,
            ICollection<ExposureRouteType> exposureRoutes,
            Compound compound,
            KineticModelInstance kineticModelInstance,
            bool isAcute
        ) {
            ModelCode = kineticModelInstance.IdModelDefinition;
            SubstanceCode = compound.Code;
            SubstanceName = compound.Name;
            NumberOfDays = kineticModelInstance.NumberOfDays;
            NumberOfDosesPerDay = kineticModelInstance.NumberOfDosesPerDay;
            NumberOfDaysSkipped = kineticModelInstance.NonStationaryPeriod >= NumberOfDays ? 0 : kineticModelInstance.NonStationaryPeriod;
            getModelInstanceSettings(kineticModelInstance, kineticModelInstance.CodeCompartment, exposureRoutes);
            InternalTargetSystemExposures = drillDownRecords.Select(model => getDrillDownCompoundIndividualExposure(model, compound, exposureRoutes)).ToList();
            Maximum = InternalTargetSystemExposures.Max(c => c.MaximumTargetExposure);
            IsAcute = isAcute;
        }

        public void SummarizeIndividualDayDrillDown(
            ICollection<ITargetIndividualDayExposure> drillDownDayRecords,
            ICollection<ExposureRouteType> exposureRoutes,
            Compound compound,
            KineticModelInstance kineticModelInstance
        ) {
            ModelCode = kineticModelInstance.IdModelDefinition;
            SubstanceCode = compound.Code;
            SubstanceName = compound.Name;
            NumberOfDays = kineticModelInstance.NumberOfDays;
            NumberOfDosesPerDay = kineticModelInstance.NumberOfDosesPerDay;
            NumberOfDaysSkipped = kineticModelInstance.NonStationaryPeriod >= NumberOfDays ? 0 : kineticModelInstance.NonStationaryPeriod;
            getModelInstanceSettings(kineticModelInstance, kineticModelInstance.CodeCompartment, exposureRoutes);
            InternalTargetSystemExposures = drillDownDayRecords.Select(model => getDrillDownCompoundIndividualDayExposure(model, compound, exposureRoutes)).ToList();
            Maximum = InternalTargetSystemExposures.Max(c => c.MaximumTargetExposure);
            IsAcute = true;
        }

        private void getModelInstanceSettings(KineticModelInstance kineticModelInstance, string codeCompartment, ICollection<ExposureRouteType> exposureRoutes) {
            if (kineticModelInstance != null) {
                var timeUnit = kineticModelInstance.ResolutionType;
                TimeUnit = timeUnit.GetShortDisplayName();
                if (timeUnit == General.TimeUnit.Hours) {
                    StepLength = 60 / kineticModelInstance.KineticModelDefinition.EvaluationFrequency;
                } else {
                    StepLength = 1 / kineticModelInstance.KineticModelDefinition.EvaluationFrequency;
                }
                ModelName = kineticModelInstance.KineticModelDefinition.Name;
                ModelDescription = kineticModelInstance.KineticModelDefinition.Description;
                DoseUnit = kineticModelInstance.KineticModelDefinition.Outputs.Single(c => c.Id == codeCompartment).DoseUnit.GetShortDisplayName();
                OutputCode = kineticModelInstance.CodeCompartment;
                OutputDescription = kineticModelInstance.KineticModelDefinition.Outputs.Single(c => c.Id == codeCompartment).Description;
                ExposureRoutes = exposureRoutes.Select(c => c.GetShortDisplayName()).ToList();
            }
        }

        public static ICollection<ITargetIndividualExposure> GetDrilldownIndividualTargetExposures(
            ICollection<AggregateIndividualExposure> individualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForDrilldown,
            bool isPerPerson
        ) {
            var intakes = individualExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = individualExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, percentageForDrilldown);
            var referenceIndividualIndex = intakes.Count(c => c < weightedPercentileValue);

            var lowerExtremePerson = _specifiedTakeNumer - 1;
            if (percentageForDrilldown != 100) {
                lowerExtremePerson = BMath.Floor(_specifiedTakeNumer / 2);
            }

            var result = individualExposures
                .OrderBy(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(_specifiedTakeNumer)
                .Cast<ITargetIndividualExposure>()
                .ToList();

            return result;
        }

        public static ICollection<ITargetIndividualDayExposure> GetDrilldownIndividualDayTargetExposures(
            ICollection<AggregateIndividualDayExposure> individualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForDrilldown,
            bool isPerPerson
        ) {
            var intakes = individualDayExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = individualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, percentageForDrilldown);
            var referenceIndividualIndex = intakes.Count(c => c < weightedPercentileValue);

            var lowerExtremePerson = _specifiedTakeNumer - 1;
            if (percentageForDrilldown != 100) {
                lowerExtremePerson = BMath.Floor(_specifiedTakeNumer / 2);
            }

            var result = individualDayExposures
                .OrderBy(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(_specifiedTakeNumer)
                .Cast<ITargetIndividualDayExposure>()
                .ToList();

            return result;
        }

        private InternalExposuresPerIndividual getDrillDownCompoundIndividualExposure(ITargetIndividualExposure targetExposure, Compound compound, ICollection<ExposureRouteType> exposureRoutes) {
            var result = new InternalExposuresPerIndividual() {
                Weight = targetExposure.Individual.BodyWeight,
                Code = targetExposure.Individual.Code,
                RelativeCompartmentWeight = targetExposure.RelativeCompartmentWeight,
                Covariable = targetExposure.Individual.Covariable,
                Cofactor = targetExposure.Individual.Cofactor,
                TargetExposure = targetExposure.GetExposureForSubstance(compound),
            };
            var compoundTargetSystemExposurePattern = targetExposure.GetSubstanceTargetExposure(compound) as SubstanceTargetExposurePattern;

            if (compoundTargetSystemExposurePattern != null) {
                result.MaximumTargetExposure = compoundTargetSystemExposurePattern.MaximumTargetExposure;
                result.ExposurePerRoute = new Dictionary<string, double>();
                foreach (var route in exposureRoutes) {
                    result.ExposurePerRoute[route.ToString()] = 0;
                }
                if (targetExposure is AggregateIndividualExposure) {
                    result.ExternalExposure = (targetExposure as AggregateIndividualExposure)
                        .ExternalIndividualDayExposures
                        ?.Select(c => {
                            var externalExposureDays = (targetExposure as AggregateIndividualExposure).ExternalIndividualDayExposures.Count;
                            var exposure = 0d;
                            foreach (var route in exposureRoutes) {
                                if (c.ExposuresPerRouteSubstance.ContainsKey(route)) {
                                    var exposurePerRoute = c.ExposuresPerRouteSubstance[route].Where(s => s.Compound == compound).Sum(s => s.Exposure) / externalExposureDays;
                                    result.ExposurePerRoute[route.ToString()] += exposurePerRoute;
                                    exposure += exposurePerRoute;
                                }
                            }
                            return exposure;
                        })
                        .Sum() ?? 0;
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

        private InternalExposuresPerIndividual getDrillDownCompoundIndividualDayExposure(ITargetIndividualDayExposure targetExposure, Compound compound, ICollection<ExposureRouteType> exposureRoutes) {
            var result = new InternalExposuresPerIndividual() {
                Weight = targetExposure.Individual.BodyWeight,
                Code = targetExposure.Individual.Code,
                RelativeCompartmentWeight = targetExposure.RelativeCompartmentWeight,
                Covariable = targetExposure.Individual.Covariable,
                Cofactor = targetExposure.Individual.Cofactor,
                TargetExposure = targetExposure.GetExposureForSubstance(compound),
            };
            var compoundTargetSystemExposurePattern = targetExposure.GetSubstanceTargetExposure(compound) as SubstanceTargetExposurePattern;
            if (compoundTargetSystemExposurePattern != null) {
                result.MaximumTargetExposure = compoundTargetSystemExposurePattern.MaximumTargetExposure;
                result.ExposurePerRoute = new Dictionary<string, double>();
                foreach (var route in exposureRoutes) {
                    result.ExposurePerRoute[route.ToString()] = 0;
                }
                if (targetExposure is AggregateIndividualDayExposure) {
                    var exposure = 0d;
                    foreach (var route in exposureRoutes) {
                        var exposurePerRoute = (targetExposure as AggregateIndividualDayExposure).ExposuresPerRouteSubstance[route]
                            .Where(s => s.Compound == compound)
                            .Sum(s => s.Exposure);
                        result.ExposurePerRoute[route.ToString()] += exposurePerRoute;
                        exposure += exposurePerRoute;
                    }
                    result.ExternalExposure = exposure;
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
    }
}
