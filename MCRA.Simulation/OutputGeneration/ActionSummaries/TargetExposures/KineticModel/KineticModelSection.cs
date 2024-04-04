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
        public List<double> SteadyStateTargetExposures { get; set; }
        public List<double> PeakTargetExposures { get; set; }
        public List<double> ExternalExposures { get; set; }
        public ExposureType ExposureType { get; set; }
        public double ConcentrationRatioPeak { get; set; }
        public double ConcentrationRatioAverage { get; set; }
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
                     .Single(c => c.Id == kineticModelInstance.CodeCompartment).Description,
                OutputUnit = kineticModelInstance.KineticModelDefinition.Outputs
                     .Single(c => c.Id == kineticModelInstance.CodeCompartment).DoseUnit.GetShortDisplayName(),
                TimeUnit = kineticModelInstance.ResolutionType.GetShortDisplayName(),
                NumberOfDosesPerDay = kineticModelInstance.NumberOfDosesPerDay,
                NumberOfDaysSkipped = kineticModelInstance.NonStationaryPeriod >= kineticModelInstance.NumberOfDays ? 0 : kineticModelInstance.NonStationaryPeriod,
                NumberOfExposureDays = kineticModelInstance.NumberOfDays,
            };
            KineticModelRecords.Add(kineticModelRecord);
        }

        public void SummarizeAbsorptionChart(
            ICollection<ITargetExposure> targetExposures,
            Compound compound,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureType exposureType
        ) {
            ExposureType = exposureType;
            var exposures = exposureType == ExposureType.Chronic
                ? targetExposures.Cast<AggregateIndividualExposure>()
                : targetExposures.Cast<AggregateIndividualDayExposure>();

            var substanceTargetExposures = exposures
                .Where(r => r.TargetExposuresBySubstance[compound] is SubstanceTargetExposurePattern)
                .Select(r => (
                    Individual: r.Individual,
                    CompartmentWeight: r.CompartmentWeight,
                    ExternalIndividualDayExposures: r.ExternalIndividualDayExposures,
                    ExposuresPerRouteSubstance: r.ExposuresPerRouteSubstance,
                    SubstanceExposurePattern: r.TargetExposuresBySubstance[compound] as SubstanceTargetExposurePattern
                ))
                .ToList();
            if (substanceTargetExposures.Any()) {
                PeakTargetExposures = substanceTargetExposures
                    .Select(c => c.SubstanceExposurePattern.PeakTargetExposure / c.CompartmentWeight)
                    .ToList();
                SteadyStateTargetExposures = substanceTargetExposures
                    .Select(c => c.SubstanceExposurePattern.SteadyStateTargetExposure / c.CompartmentWeight)
                    .ToList();
                if (exposureType == ExposureType.Chronic) {
                    ExternalExposures = substanceTargetExposures
                        .SelectMany(c => c.ExternalIndividualDayExposures)
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => {
                            var exposureAmount = 0d;
                            foreach (var route in exposureRoutes) {
                                exposureAmount += c.SelectMany(s => s.ExposuresPerRouteSubstance[route])
                                    .Where(s => s.Compound == compound)
                                    .Sum(s => s.Exposure) / c.Count();
                            }
                            return exposureAmount / c.First().Individual.BodyWeight;
                        })
                        .ToList();
                } else {
                    ExternalExposures = substanceTargetExposures
                        .Select(c => {
                            var exposureAmount = 0d;
                            foreach (var route in exposureRoutes) {
                                exposureAmount += c.ExposuresPerRouteSubstance[route]
                                    .Where(s => s.Compound == compound)
                                    .Sum(s => s.Exposure);
                            }
                            return exposureAmount / c.Individual.BodyWeight;
                        })
                        .ToList();
                }
            }
        }

        public void SummarizeAbsorptionFactors(IDictionary<(ExposurePathType, Compound), double> absorptionFactors, Compound compound, ICollection<ExposurePathType> exposureRoutes) {
            AbsorptionFactorsPercentiles = new List<UncertainDataPointCollection<double>>();
            foreach (var route in exposureRoutes) {
                if (!absorptionFactors.TryGetValue((route, compound), out var factor)) {
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

        public void SummarizeAbsorptionFactorsUncertainty(IDictionary<(ExposurePathType, Compound), double> absorptionFactors, Compound compound, ICollection<ExposurePathType> exposureRoutes) {
            var counter = 0;
            foreach (var route in exposureRoutes) {
                if (!absorptionFactors.TryGetValue((route, compound), out var factor)) {
                    factor = double.NaN;
                }
                AbsorptionFactorsPercentiles[counter].AddUncertaintyValues(new[] { factor });
                counter++;
            }
        }
    }
}
