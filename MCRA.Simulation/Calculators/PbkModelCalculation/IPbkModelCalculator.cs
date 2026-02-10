using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbkModelCalculation {
    public interface IPbkModelCalculator {
        KineticModelInstance KineticModelInstance { get; }
        List<Compound> OutputSubstances { get; }
        PbkSimulationSettings SimulationSettings { get; }
        Compound Substance { get; }

        List<PbkSimulationOutput> Calculate(
            ICollection<(SimulatedIndividual Individual, List<IExternalIndividualDayExposure> IndividualDayExposures)> externalIndividualExposures,
            ExposureUnitTriple externalExposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            IRandom generator,
            ProgressState progressState = null
        );
    }
}
