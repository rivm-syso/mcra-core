using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public sealed class ExternalIndividualExposure(SimulatedIndividual individual) : IExternalIndividualExposure {
        public SimulatedIndividual SimulatedIndividual { get; } = individual;
        public Dictionary<ExposurePath, List<IIntakePerCompound>> ExposuresPerPath { get; set; }

        /// <summary>
        /// Returns true if this individual day exposure contains one or more positive amounts for the specified
        /// route and substance.
        /// </summary>
        public bool HasPositives(ExposureRoute route, Compound substance) {
            return ExposuresPerPath
                .Where(e => e.Key.Route == route)
                .SelectMany(k => k.Value)
                .Where(i => i.Compound == substance)
                .Any(i => i.Amount > 0);
        }

        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
