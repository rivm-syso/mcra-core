using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {

    public class AggregateIndividualExposure : TargetIndividualExposure, IExternalIndividualExposure {

        /// <summary>
        /// Usual external exposure per route and substance.
        /// </summary>
        public IDictionary<ExposureRouteType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }

        /// <summary>
        /// Individual day exposures per route.
        /// </summary>
        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }

        /// <summary>
        /// Returns the total intake of the substance of the dietary individual day intake.
        /// </summary>
        public double GetSubstanceTotalExposure(Compound substance) {
            return (TargetExposuresBySubstance?.ContainsKey(substance) ?? false)
                ? TargetExposuresBySubstance[substance].SubstanceAmount : 0D;
        }

        /// <summary>
        /// Computes the total dietary substance exposures per mass-unit on this individual-day.
        /// </summary>
        public double GetSubstanceTotalExposurePerMassUnit(
            Compound substance,
            bool isPerPerson
        ) {
            var result = GetSubstanceTotalExposure(substance) / (isPerPerson ? 1D : RelativeCompartmentWeight * Individual.BodyWeight);
            return result;
        }
    }
}
