using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureBySourceRouteSectionBase : SummarySection{

        protected static List<(ExposurePath ExposurePath, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)>  CalculateExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var exposurePathCollection = new List<(ExposurePath ExposurePath, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();

            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            foreach (var path in paths) {
                var exposures = externalIndividualExposures
                .Select(c => (
                    SimulatedIndividual: c.SimulatedIndividual,
                    Exposure: c.ExposuresPerPath[path].Sum(r => r.Amount
                        * kineticConversionFactors[(path.Route, r.Compound)]
                        * relativePotencyFactors[r.Compound]
                        * membershipProbabilities[r.Compound]
                        / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                    ))
                ).ToList();
                exposurePathCollection.Add((path, exposures));
            }
            return exposurePathCollection;
        }
    }
}
