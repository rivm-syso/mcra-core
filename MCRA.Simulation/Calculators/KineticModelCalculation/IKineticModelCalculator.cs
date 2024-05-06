using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {

    public interface IKineticModelCalculator {

        // TODO kinetic models: How to compute kinetic conversion factors for metabolites?
        Compound InputSubstance { get; }

        List<Compound> OutputSubstances { get; }

        List<IndividualDayTargetExposureCollection> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        );

        List<IndividualTargetExposureCollection> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        );

        ISubstanceTargetExposure CalculateInternalDoseTimeCourse(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            IRandom generator
        );

        /// <summary>
        /// Computes the internal substance amount/concentration that belongs to
        /// the provided external dose of the specified exposure route. Depending
        /// on the exposure type, this may be an acute (peak) dose, or chronic
        /// (long term average) dose.
        /// If the provided dose is specified as a concentration (exposure unit),
        /// then the computed internal dose will also be a concentration. If the
        /// provided dose is an absolute amount, then the computed internal dose
        /// will also be an absolute amount.
        /// The relative compartment weight is used to convert between absolute
        /// substance amounts at the target and concentrations at the targets.
        /// </summary>
        double CalculateTargetDose(
            double dose,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            IRandom generator
        );

        /// <summary>
        /// Derives the external (daily) substance amount/concentration that produces
        /// the specified internal dose at the target.
        /// </summary>
        double Reverse(
            double dose,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            IRandom generator
        );

        /// <summary>
        /// Computes absorption factors for the different exposure routes
        /// </summary>
        IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );

        IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        );
    }
}
