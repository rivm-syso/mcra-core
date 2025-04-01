using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public sealed class ExternalIndividualExposure(SimulatedIndividual individual) : IExternalIndividualExposure {
        public SimulatedIndividual SimulatedIndividual { get; } = individual;
        public Dictionary<ExposurePath, List<IIntakePerCompound>> ExposuresPerPath { get; set; }
        public List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }

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

        /// <summary>
        /// Gets the total substance exposure summed for the specified route
        /// of the simulated individual.
        /// </summary>
        public double GetExposure(
            ExposureRoute route,
            Compound substance,
            bool isPerPerson
        ) {
            var totalAmount = ExposuresPerPath
                .Where(e => e.Key.Route == route)
                .SelectMany(k => k.Value)
                .Where(i => i.Compound == substance)
                .Sum(i => i.Amount);
            return isPerPerson ? totalAmount : totalAmount / SimulatedIndividual.BodyWeight;
        }

        /// <summary>
        /// Gets the total substance exposure summed for the specified route and
        /// optionally corrected for the body weight.
        /// </summary>
        public double GetExposure(
            ExposurePath path,
            Compound substance,
            bool isPerPerson
        ) {
            if (ExposuresPerPath.TryGetValue(path, out var pathExposures)) {
                var totalAmount = pathExposures
                    .Where(i => i.Compound == substance)
                    .Sum(i => i.Amount * SimulatedIndividual.SamplingWeight);
                return isPerPerson ? totalAmount : totalAmount / SimulatedIndividual.BodyWeight;
            }
            return 0;
        }
    }
}
