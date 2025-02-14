using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public interface ITargetExposuresCalculator {

        ICollection<AggregateIndividualDayExposure> ComputeAcute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            IRandom generator,
            ProgressState progressState
        );

        ICollection<AggregateIndividualExposure> ComputeChronic(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            IRandom generator,
            ProgressState progressState
        );
    }
}
