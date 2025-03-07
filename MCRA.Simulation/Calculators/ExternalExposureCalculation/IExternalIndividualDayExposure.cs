using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    /// <summary>
    /// External exposures for an individual at a certain day, originating from different sources and routes.
    /// </summary>
    public interface IExternalIndividualDayExposure : IIndividualDay {

        abstract Dictionary<ExposurePath, List<IIntakePerCompound>> ExposuresPerPath { get; }

        /// <summary>
        /// Returns true if this individual day exposure contains one or more positive amounts for a route and substance.
        /// </summary>
        bool HasPositives(ExposureRoute route, Compound substance);
        
        /// <summary>
        /// Gets the total exposure summed for a substance, and optionally corrected for the body weight.
        /// </summary>
        double GetExposure(
            Compound substance,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the total exposure for a substance and route.
        /// </summary>
        double GetExposure(
            ExposureRoute route,
            Compound substance
        );

        /// <summary>
        /// Get the total exposure summed for the specified route and substance, optionally corrected for the body weight.
        /// </summary>
        double GetExposure(
            ExposureRoute route,
            Compound substance,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the total external exposure summed over substances (using RPFs and memberships)
        /// and summed over the different routes and sources.
        /// </summary>
        double GetExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the total (cumulative) exposure for the specified route.
        /// </summary>
        double GetExposure(
            ExposureRoute route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the total external exposure of the specified substance multiplied by the kinetic conversion factors.
        /// </summary>
        double GetExposure(
            Compound substance,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        );

        /// <summary>
        /// Get the kinetic-converted total exposure for the specified route, corrected for RPFs and memberships, 
        /// summed for all substances, and optionally corrected for the body weight.
        /// </summary>
        double GetExposure(
            ExposureRoute route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        );

        /// <summary>
        /// Gets the total external exposure summed over substances (using RPFs and memberships)
        /// and summed over the different routes and sources, using kinetic conversion factors per route.
        /// TODO: this method should be removed or refactored. Use of a dictionary of kinetic
        /// conversion factors per route should not be done when summing external exposures.
        /// </summary>
        double GetExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        );

        /// <summary>
        /// Get exposures by substance, where the exposure value for a substance is summed from different sources
        /// and routes.
        /// </summary>
        ICollection<IIntakePerCompound> GetExposuresBySubstance();

        /// <summary>
        /// Get exposures by substance for a specified route, where the exposure value for a substance is summed 
        /// from different sources.
        /// </summary>
        ICollection<IIntakePerCompound> GetExposuresBySubstance(
            ExposureRoute exposureRoute
        );
    }
}
