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
            ExposureUnitTriple exposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        );

        ICollection<TargetIndividualExposure> ComputeTargetIndividualExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposureRouteType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        );

        IDictionary<(ExposureRouteType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );

        IDictionary<(ExposureRouteType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );
    }
}
