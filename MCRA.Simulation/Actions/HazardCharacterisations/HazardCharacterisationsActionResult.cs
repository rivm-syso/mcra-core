using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.Actions.HazardCharacterisations {
    public sealed class HazardCharacterisationsActionResult : IActionResult {
        public TargetUnit TargetDoseUnit { get; set; }
        public PointOfDepartureType HazardCharacterisationType { get; set; }
        public ICollection<ExposureRoute> ExposureRoutes { get; set; }
        public List<HazardCharacterisationModelsCollection> HazardCharacterisationsFromPodAndBmd { get; } = new();
        public List<IviveHazardCharacterisation> HazardCharacterisationsFromIvive { get; } = new();
        public List<IHazardCharacterisationModel> ImputedHazardCharacterisations { get; } = new();
        public List<IHazardCharacterisationModel> HazardCharacterisationImputationRecords { get; } = new();
        public List<HazardCharacterisationModelCompoundsCollection> HazardCharacterisationModelsCollections { get; } = new();
        public List<(AggregateIndividualExposure AggregateIndividualExposure, IHazardCharacterisationModel HcModel)> KineticModelDrilldownRecords{ get; } = new();
        public IUncertaintyFactorialResult FactorialResult { get; set; }
        public Compound ReferenceSubstance { get ; set; }   
    }
}
