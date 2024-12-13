using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface IExternalIndividualDayExposure : IIndividualDay {

        Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; }

        /// <summary>
        /// Gets the total external exposure summed over substances (using RPFs and memberships)
        /// and summed over the different routes.
        /// </summary>
        double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the total external exposure of the substance summed over the different routes.
        /// </summary>
        double GetTotalExternalExposureForSubstance(
            Compound substance,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the total external exposure summed over substances (using RPFs and memberships)
        /// and summed over the different routes (using kinetic conversion factors per route).
        /// TODO: this method should be removed or refactored. Use of a dictionary of kinetic
        /// conversion factors per route should not be done when summing external exposures.
        /// </summary>
        double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        );

        double GetTotalExternalExposureForSubstance(
            Compound substance,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        );

        double GetTotalRouteExposure(
            ExposurePathType route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        );

        double GetTotalRouteExposure(
            ExposurePathType route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        );

        double GetSubstanceExposureForRoute(
            ExposurePathType route,
            Compound substance,
            bool isPerPerson
        );

        ICollection<IIntakePerCompound> GetTotalExposurePerCompound();

        ICollection<IIntakePerCompound> GetTotalExposurePerRouteSubstance(
            ExposureRoute exposureRoute
        );
    }
}
