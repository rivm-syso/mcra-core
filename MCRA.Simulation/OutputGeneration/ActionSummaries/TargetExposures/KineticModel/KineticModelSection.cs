using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class KineticModelSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<UncertainDataPointCollection<double>> AbsorptionFactorsPercentiles { get; set; }
        public List<KineticModelRecord> KineticModelRecords { get; set; } = new();
        public List<string> AllExposureRoutes { get; set; } = new();
        public List<(string compartment, List<double>)> SteadyStateTargetExposures { get; set; } = new();
        public List<(string compartment, List<double>)> PeakTargetExposures { get; set; } = new();
        public List<(string compartment, List<double>)> ExternalExposures { get; set; } = new();
        public ExposureType ExposureType { get; set; }
        public string SubstanceName { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        public void Summarize(
            Compound substance,
            KineticModelInstance kineticModelInstance,
            ICollection<ExposurePathType> exposureRoutes,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            SubstanceName = substance.Name;
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;

            var kineticModelRecord = new KineticModelRecord() {
                ModelName = kineticModelInstance.KineticModelDefinition.Name,
                ModelCode = kineticModelInstance.IdModelInstance,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                DoseUnit = string.Join(", ", kineticModelInstance.KineticModelDefinition.Forcings
                     .Select(r => r.DoseUnit.GetShortDisplayName()).Distinct()),
                Routes = string.Join(", ", exposureRoutes.Select(c => c.GetShortDisplayName())),
                Output = kineticModelInstance.KineticModelDefinition.Outputs
                     .Single(c => c.Id == kineticModelInstance.CompartmentCodes[0]).Id,
                OutputUnit = kineticModelInstance.KineticModelDefinition.Outputs
                     .Single(c => c.Id == kineticModelInstance.CompartmentCodes[0]).DoseUnit.GetShortDisplayName(),
                TimeUnit = kineticModelInstance.ResolutionType.GetShortDisplayName(),
                NumberOfDosesPerDay = kineticModelInstance.NumberOfDosesPerDay,
                NumberOfDaysSkipped = kineticModelInstance.NonStationaryPeriod >= kineticModelInstance.NumberOfDays ? 0 : kineticModelInstance.NonStationaryPeriod,
                NumberOfExposureDays = kineticModelInstance.NumberOfDays,
            };
            KineticModelRecords.Add(kineticModelRecord);
        }

        public void SummarizeAbsorptionChart(
            ICollection<ITargetExposure> targetExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureType exposureType,
            IEnumerable<string> compartments
        ) {
            ExposureType = exposureType;
            var exposures = exposureType == ExposureType.Chronic
                ? targetExposures.Cast<AggregateIndividualExposure>()
                : targetExposures.Cast<AggregateIndividualDayExposure>();

            var substanceTargetExposures = exposures
                .Where(r => r.TargetExposuresBySubstance[substance] is SubstanceTargetExposurePattern)
                .Select(r => (
                    r.Individual,
                    r.RelativeCompartmentWeight,
                    r.ExternalIndividualDayExposures,
                    r.ExposuresPerRouteSubstance,
                    SubstanceExposurePattern: (SubstanceTargetExposurePattern)r.TargetExposuresBySubstance[substance]
                ))
                .ToList();

            if (substanceTargetExposures.Any()) {
                foreach (var compartment in compartments) {
                    var peakExposures = substanceTargetExposures
                        .Where(c => c.SubstanceExposurePattern.CompartmentInfo.compartment == compartment)
                        .Select(c => c.SubstanceExposurePattern.PeakTargetExposure / (c.Individual.BodyWeight * c.RelativeCompartmentWeight))
                        .ToList();
                    PeakTargetExposures.Add((compartment, peakExposures));

                    var steadyStateExposures = substanceTargetExposures
                        .Where(c => c.SubstanceExposurePattern.CompartmentInfo.compartment == compartment)
                        .Select(c => c.SubstanceExposurePattern.SteadyStateTargetExposure / (c.Individual.BodyWeight * c.RelativeCompartmentWeight))
                        .ToList();
                    SteadyStateTargetExposures.Add((compartment, steadyStateExposures));
                    if (exposureType == ExposureType.Chronic) {
                        var externalExposures = substanceTargetExposures
                            .SelectMany(c => c.ExternalIndividualDayExposures)
                            .GroupBy(c => c.SimulatedIndividualId)
                            .Select(c => {
                                var exposureAmount = 0d;
                                foreach (var route in exposureRoutes) {
                                    exposureAmount += c.SelectMany(s => s.ExposuresPerRouteSubstance[route])
                                        .Where(s => s.Compound == substance)
                                        .Sum(s => s.Amount) / c.Count();
                                }
                                return exposureAmount / c.First().Individual.BodyWeight;
                            })
                            .ToList();
                        ExternalExposures.Add((compartment, externalExposures));
                    } else {
                        var externalExposures = substanceTargetExposures
                            .Select(c => {
                                var exposureAmount = 0d;
                                foreach (var route in exposureRoutes) {
                                    exposureAmount += c.ExposuresPerRouteSubstance[route]
                                        .Where(s => s.Compound == substance)
                                        .Sum(s => s.Amount);
                                }
                                return exposureAmount / c.Individual.BodyWeight;
                            })
                            .ToList();
                        ExternalExposures.Add((compartment, externalExposures));
                    }
                }
            }
        }

        public void SummarizeAbsorptionFactors(
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors, 
            Compound substance, 
            ICollection<ExposurePathType> exposureRoutes
        ) {
            AbsorptionFactorsPercentiles = new List<UncertainDataPointCollection<double>>();
            foreach (var route in exposureRoutes) {
                if (!absorptionFactors.TryGetValue((route, substance), out var factor)) {
                    factor = double.NaN;
                }
                var absorptionFactorsPercentile = new UncertainDataPointCollection<double>() {
                    XValues = new[] { 0D },
                    ReferenceValues = new[] { factor }
                };
                AbsorptionFactorsPercentiles.Add(absorptionFactorsPercentile);
                AllExposureRoutes.Add($"{route.GetShortDisplayName()}");
            }
        }
        public void SummarizeAbsorptionFactorsUncertainty(
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors, 
            Compound substance, 
            ICollection<ExposurePathType> exposureRoutes
        ) {
            var counter = 0;
            foreach (var route in exposureRoutes) {
                if (!absorptionFactors.TryGetValue((route, substance), out var factor)) {
                    factor = double.NaN;
                }
                AbsorptionFactorsPercentiles[counter].AddUncertaintyValues(new[] { factor });
                counter++;
            }
        }
    }
}
