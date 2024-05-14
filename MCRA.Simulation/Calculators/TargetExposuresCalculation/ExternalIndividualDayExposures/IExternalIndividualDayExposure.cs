using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface IExternalIndividualDayExposure {
        int SimulatedIndividualId { get; }
        int SimulatedIndividualDayId { get; }
        Individual Individual { get; }
        string Day { get; }
        double IndividualSamplingWeight { get; set; }
        Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }

        double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        );

        double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        );

        double GetTotalExternalExposureForSubstance(
            Compound substance,
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
    }
}
