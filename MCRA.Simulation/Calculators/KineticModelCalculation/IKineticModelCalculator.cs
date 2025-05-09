﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {

    public interface IKineticModelCalculator {

        /// <summary>
        /// The substance for which the kinetic conversion is calculated.
        /// </summary>
        Compound Substance { get; }

        /// <summary>
        /// A collection of one or more output substances that results from the kinetic
        /// conversion. A kinetic conversion may simulate the creation of new metabolites
        /// from the input substance but can also be a simple input-output conversion for
        /// the same substance.
        /// </summary>
        List<Compound> OutputSubstances { get; }

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
        /// Computes absorption factors for the different exposure routes.
        /// </summary>
        IDictionary<ExposureRoute, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        );

        IDictionary<ExposureRoute, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        );
    }
}
