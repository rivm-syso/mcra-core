using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation;

namespace MCRA.Simulation.Actions.TargetExposures {
    public sealed class TargetExposuresActionResult : IActionResult {
        public ExposureUnitTriple ExternalExposureUnit { get; set; }
        public TargetUnit TargetExposureUnit { get; set; }
        public ICollection<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
        public ICollection<IExternalIndividualExposure> ExternalIndividualExposures { get; set; }
        public ICollection<ExternalExposureCollection> ExternalExposureCollections { get; set; }
        public ICollection<KineticConversionFactorResultRecord> KineticConversionFactors { get; set; }
        public ICollection<ExposureRoute> ExposureRoutes { get; set; }
        public ICollection<AggregateIndividualDayExposure> AggregateIndividualDayExposures { get; set; }
        public ICollection<AggregateIndividualExposure> AggregateIndividualExposures { get; set; }
        public IDictionary<Compound, IKineticConversionCalculator> KineticConversionCalculators { get; set; }
        public ExposureMatrix ExposureMatrix { get; set; }
        public List<DriverSubstance> DriverSubstances { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }

        public IDictionary<(ExposureRoute, Compound), double> KineticConversionFactorsByRouteSubstance {
            get {
                var factorsGrouped = KineticConversionFactors
                    .GroupBy(r => (r.ExposureRoute, r.Substance));
                if (factorsGrouped.Any(r => r.Count() > 1)) {
                    throw new NotImplementedException("Kinetic conversion factors are not unique by route and substance.");
                }
                return factorsGrouped.ToDictionary(r => r.Key, r => r.Single().Factor);
            }
        }
    }
}
