using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public interface ITargetExposuresCalculator {

        ICollection<TargetIndividualDayExposureCollection> ComputeTargetIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            IRandom generator,
            ProgressState progressState
        );

        ICollection<TargetIndividualExposureCollection> ComputeTargetIndividualExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            IRandom generator,
            ProgressState progressState
        );

        IDictionary<(ExposurePathType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );

        IDictionary<(ExposurePathType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );
    }
}
