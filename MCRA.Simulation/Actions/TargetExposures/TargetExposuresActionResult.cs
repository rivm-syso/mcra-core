using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.TargetExposures {
    public sealed class TargetExposuresActionResult : IActionResult {

        public ExposureUnitTriple ExternalExposureUnit { get; set; }
        public TargetUnit TargetExposureUnit { get; set; }

        public ICollection<NonDietaryIndividualDayIntake> NonDietaryIndividualDayIntakes { get; set; }

        public IDictionary<(ExposurePathType, Compound), double> KineticConversionFactors { get; set; }
        public ICollection<ExposurePathType> ExposureRoutes { get; set; }

        public ICollection<AggregateIndividualDayExposure> AggregateIndividualDayExposures { get; set; }
        public ICollection<AggregateIndividualExposure> AggregateIndividualExposures { get; set; }

        public IDictionary<Compound, IKineticModelCalculator> KineticModelCalculators { get; set; }

        public ExposureMatrix ExposureMatrix { get; set; }
        public List<DriverSubstance> DriverSubstances { get; set; }

        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
