using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {

    public interface IHazardCharacterisationImputationCalculator {

        IHazardCharacterisationModel ImputeNominal(
            Compound substance,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            IRandom kineticModelRandomGenerator
        );

        ICollection<IHazardCharacterisationModel> ImputeNominal(
            ICollection<Compound> substances,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            IRandom kineticModelRandomGenerator
        );

        IHazardCharacterisationModel ImputeUncertaintyRun(
            Compound substance,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            IRandom generator,
            IRandom kineticModelRandomGenerator
        );

        ICollection<IHazardCharacterisationModel> ImputeUncertaintyRun(
            ICollection<Compound> substances,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            IRandom generator,
            IRandom kineticModelRandomGenerator
        );

        ICollection<IHazardCharacterisationModel> GetImputationRecords();
    }
}
