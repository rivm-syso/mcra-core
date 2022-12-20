using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.HazardCharacterisations {
    public sealed class HazardCharacterisationsActionResult : IActionResult {
        public TargetUnit TargetDoseUnit { get; set; }
        public PointOfDepartureType HazardCharacterisationType { get; set; }
        public ICollection<ExposureRouteType> ExposureRoutes { get; set; }
        public ICollection<IHazardCharacterisationModel> HazardCharacterisationsFromPodAndBmd { get; set; }
        public ICollection<IviveHazardCharacterisation> HazardCharacterisationsFromIvive { get; set; }
        public ICollection<IHazardCharacterisationModel> ImputedHazardCharacterisations { get; set; }
        public ICollection<IHazardCharacterisationModel> HazardCharacterisationImputationRecords { get; set; }
        public IDictionary<Compound, IHazardCharacterisationModel> HazardCharacterisations { get; set; }
        public List<AggregateIndividualExposure> KineticModelDrilldownRecords{ get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
