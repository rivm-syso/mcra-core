using MCRA.Utils.Collections;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public interface ITargetExposuresCalculator {
        ICollection<TargetIndividualDayExposure> ComputeTargetIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit targetExposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        );

        ICollection<TargetIndividualExposure> ComputeTargetIndividualExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit targetExposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        );

        TwoKeyDictionary<ExposureRouteType, Compound, double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            TargetUnit targetExposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );

        TwoKeyDictionary<ExposureRouteType, Compound, double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            TargetUnit targetExposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );
    }
}
