using System.ComponentModel.DataAnnotations;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionCalculation {

    public enum KineticConversionModelType {
        [Display(Name = "Kinetic conversion factor model")]
        ConversionFactorModel,
        [Display(Name = "PBK model")]
        PbkModel
    }

    public interface IKineticConversionCalculator {

        KineticConversionModelType ModelType { get; }

        List<AggregateIndividualDayExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        );

        List<AggregateIndividualExposure> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        );

        /// <summary>
        /// Computes the internal substance amount/concentration that belongs to
        /// the provided external dose of the specified exposure route. Depending
        /// on the exposure type, this may be an acute (peak) dose, or chronic
        /// (long term average) dose.
        /// </summary>
        double Forward(
            SimulatedIndividual individual,
            double externalDose,
            ExposureRoute route,
            ExposureUnitTriple exposureUnit,
            TargetUnit internalTargetUnit,
            ExposureType exposureType,
            IRandom generator
        );

        ISubstanceTargetExposure Forward(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            ExposureRoute route,
            ExposureUnitTriple exposureUnit,
            TargetUnit internalTargetUnit,
            ExposureType exposureType,
            IRandom generator
        );

        /// <summary>
        /// Derives the external (daily) substance amount/concentration that produces
        /// the specified internal dose at the target.
        /// </summary>
        double Reverse(
            SimulatedIndividual individual,
            double internalDose,
            TargetUnit internalDoseUnit,
            ExposureRoute route,
            ExposureUnitTriple exposureUnit,
            ExposureType exposureType,
            IRandom generator
        );

        /// <summary>
        /// Computes kinetic conversion factors for acute individual day exposures
        /// for the different exposure routes.
        /// </summary>
        List<KineticConversionFactorResultRecord> ComputeKineticConversionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        );

        /// <summary>
        /// Computes kinetic conversion factors for chronic individual exposures
        /// for the different exposure routes.
        /// </summary>
        List<KineticConversionFactorResultRecord> ComputeKineticConversionFactors(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        );
    }
}
