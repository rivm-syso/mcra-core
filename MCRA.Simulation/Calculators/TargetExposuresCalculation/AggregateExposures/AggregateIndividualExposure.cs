using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {

    public class AggregateIndividualExposure : TargetIndividualExposure, IExternalIndividualExposure {

        /// <summary>
        /// Usual external exposure per route and substance.
        /// </summary>
        public IDictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }

        /// <summary>
        /// Individual day exposures per route.
        /// </summary>
        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }

        public AggregateIndividualExposure Clone() {
            return new AggregateIndividualExposure() {
                ExposuresPerRouteSubstance = ExposuresPerRouteSubstance,
                Individual = Individual,
                ExternalIndividualDayExposures = ExternalIndividualDayExposures,
                IndividualSamplingWeight = IndividualSamplingWeight,
                SimulatedIndividualId = SimulatedIndividualId,
                RelativeCompartmentWeight = RelativeCompartmentWeight,
            };
        }
    }
}
