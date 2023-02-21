using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class KineticModelSection : ActionSummaryBase {

        public List<UncertainDataPointCollection<double>> AbsorptionFactorsPercentiles { get; set; }

        public List<string> AllExposureRoutes { get; set; }

        public string CodeModel { get; set; }
        public string Description { get; set; }
        public string TargetOrgan { get; set; }
        public string OutputDescription { get; set; }
        public string CodeSubstance { get; set; }

        public string TimeUnit { get; set; }
        public List<string> ExposureRoutes { get; set; }

        public List<double> TargetExposures { get; set; }
        public List<double> SteadyStateTargetExposures { get; set; }
        public List<double> PeakTargetExposures { get; set; }
        public List<double> ExternalExposures { get; set; }
        public double Maximum { get; set; }

        public double ConcentrationRatioPeak { get; set; }
        public double ConcentrationRatioAverage { get; set; }
        public bool IsAcute { get; set; }

        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        public void Summarize(
            Compound substance,
            KineticModelInstance kineticModelInstance,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            CodeSubstance = substance.Code;
            CodeModel = kineticModelInstance.IdModelInstance;
            TargetOrgan = kineticModelInstance.CodeCompartment;
            TimeUnit = kineticModelInstance.ResolutionType.GetShortDisplayName();
            Description = kineticModelInstance.KineticModelDefinition.Description;
            OutputDescription = kineticModelInstance.KineticModelDefinition.Outputs.Single(c => c.Id == kineticModelInstance.CodeCompartment).Description;
            ExposureRoutes = kineticModelInstance.KineticModelDefinition.Forcings.Select(c => c.Id.GetShortDisplayName()).ToList();
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
        }

        public void SummarizeAbsorptionChart(ICollection<AggregateIndividualExposure> aggregateIndividualExposures, Compound compound, ICollection<ExposureRouteType> exposureRoutes) {
            IsAcute = false;
            var compoundTargetExposures = aggregateIndividualExposures
                .Where(r => r.TargetExposuresBySubstance[compound] is SubstanceTargetExposurePattern)
                .Select(r => (
                    BodyWeight: r,
                    CompartmentWeight: r.CompartmentWeight,
                    ExternalIndividualDayExposures: r.ExternalIndividualDayExposures,
                    SubstanceExposurePattern: r.TargetExposuresBySubstance[compound] as SubstanceTargetExposurePattern
                ))
                .ToList();

            if (compoundTargetExposures.Any()) {
                TargetExposures = compoundTargetExposures
                    .Select(c => c.SubstanceExposurePattern.SubstanceAmount / c.CompartmentWeight)
                    .ToList();
                PeakTargetExposures = compoundTargetExposures
                    .Select(c => c.SubstanceExposurePattern.PeakTargetExposure / c.CompartmentWeight)
                    .ToList();
                SteadyStateTargetExposures = compoundTargetExposures
                    .Select(c => c.SubstanceExposurePattern.SteadyStateTargetExposure / c.CompartmentWeight)
                    .ToList();
                Maximum = 1.05 * compoundTargetExposures.Max(r => r.SubstanceExposurePattern.MaximumTargetExposure / r.CompartmentWeight);
                ExternalExposures = compoundTargetExposures
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
            }
        }

        public void SummarizeAbsorptionChart(ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures, Compound compound, ICollection<ExposureRouteType> exposureRoutes) {
            IsAcute = true;
            var substanceTargetExposures = aggregateIndividualDayExposures
                .Where(r => r.TargetExposuresBySubstance[compound] is SubstanceTargetExposurePattern)
                .Select(r => (
                    BodyWeight: r.Individual.BodyWeight,
                    CompartmentWeight: r.CompartmentWeight,
                    ExposuresPerRouteCompound: r.ExposuresPerRouteSubstance,
                    SubstanceExposurePattern: r.TargetExposuresBySubstance[compound] as SubstanceTargetExposurePattern
                ))
                .ToList();

            if (substanceTargetExposures.Any()) {
                TargetExposures = substanceTargetExposures
                    .Select(c => c.SubstanceExposurePattern.SubstanceAmount / c.CompartmentWeight)
                    .ToList();
                PeakTargetExposures = substanceTargetExposures
                    .Select(c => c.SubstanceExposurePattern.PeakTargetExposure / c.CompartmentWeight)
                    .ToList();
                SteadyStateTargetExposures = substanceTargetExposures
                    .Select(c => c.SubstanceExposurePattern.SteadyStateTargetExposure / c.CompartmentWeight)
                    .ToList();
                Maximum = 1.05 * substanceTargetExposures
                    .Max(r => r.SubstanceExposurePattern.MaximumTargetExposure / r.CompartmentWeight);
                ExternalExposures = substanceTargetExposures
                    .Select(c => {
                        var exposureAmount = 0d;
                        foreach (var route in exposureRoutes) {
                            exposureAmount += c.ExposuresPerRouteCompound[route]
                                .Where(s => s.Compound == compound)
                                .Sum(s => s.Exposure);
                        }
                        return exposureAmount / c.BodyWeight;
                    })
                    .ToList();
            }
        }

        public void SummarizeAbsorptionFactors(IDictionary<(ExposureRouteType, Compound), double> absorptionFactors, Compound compound, ICollection<ExposureRouteType> exposureRoutes) {
            AllExposureRoutes = new List<string>();
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

        public void SummarizeAbsorptionFactorsUncertainty(IDictionary<(ExposureRouteType, Compound), double> absorptionFactors, Compound compound, ICollection<ExposureRouteType> exposureRoutes) {
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
