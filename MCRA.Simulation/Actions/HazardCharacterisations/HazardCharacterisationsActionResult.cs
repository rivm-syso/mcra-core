using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationTimeCourseCalculation;

namespace MCRA.Simulation.Actions.HazardCharacterisations {
    public sealed class HazardCharacterisationsActionResult : IActionResult {
        public TargetUnit TargetDoseUnit { get; set; }
        public PointOfDepartureType HazardCharacterisationType { get; set; }
        public List<HazardCharacterisationModelsCollection> HazardCharacterisationsFromPodAndBmd { get; } = [];
        public List<IviveHazardCharacterisation> HazardCharacterisationsFromIvive { get; } = [];
        public List<IHazardCharacterisationModel> ImputedHazardCharacterisations { get; } = [];
        public List<IHazardCharacterisationModel> HazardCharacterisationImputationRecords { get; } = [];
        public List<HazardCharacterisationModelCompoundsCollection> HazardCharacterisationModelsCollections { get; } = [];
        public List<HazardDosePbkTimeCourse> KineticModelDrilldownRecords { get; } = [];
        public IUncertaintyFactorialResult FactorialResult { get; set; }
        public Compound ReferenceSubstance { get ; set; }
    }
}
