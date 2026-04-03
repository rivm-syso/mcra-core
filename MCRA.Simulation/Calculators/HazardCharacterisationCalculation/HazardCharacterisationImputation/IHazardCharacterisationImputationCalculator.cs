using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {

    public interface IHazardCharacterisationImputationCalculator {

        IHazardCharacterisationModel ImputeNominal(
            Compound substance,
            PointOfDepartureType targetPod,
            TargetUnit targetUnit,
            IRandom kineticModelRandomGenerator
        );

        ICollection<IHazardCharacterisationModel> ImputeNominal(
            ICollection<Compound> substances,
            PointOfDepartureType targetPod,
            TargetUnit targetUnit,
            IRandom kineticModelRandomGenerator
        );

        IHazardCharacterisationModel ImputeUncertaintyRun(
            Compound substance,
            PointOfDepartureType targetPod,
            TargetUnit targetUnit,
            IRandom generator,
            IRandom kineticModelRandomGenerator
        );

        ICollection<IHazardCharacterisationModel> ImputeUncertaintyRun(
            ICollection<Compound> substances,
            PointOfDepartureType targetPod,
            TargetUnit targetUnit,
            IRandom generator,
            IRandom kineticModelRandomGenerator
        );

        ICollection<IHazardCharacterisationModel> GetImputationRecords();
    }
}
