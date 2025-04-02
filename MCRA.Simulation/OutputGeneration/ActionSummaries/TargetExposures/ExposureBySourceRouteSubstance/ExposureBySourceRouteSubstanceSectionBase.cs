using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureBySourceRouteSubstanceSectionBase : SummarySection{

        protected static List<(ExposurePath ExposurePath, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)>  CalculateExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var exposurePathSubstanceCollection = new List<(ExposurePath ExposurePath, Compound Substance, List<(SimulatedIndividual SimulatedIndividual, double Exposure)>)>();

            var paths = externalIndividualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            foreach (var substance in substances) {
                foreach (var path in paths) {
                    var exposures = externalIndividualExposures
                    .Select(c => (
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.ExposuresPerPath[path].First(c => c.Compound.Code == substance.Code).Amount
                            * kineticConversionFactors[(path.Route, substance)]
                            / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                        )
                    ).ToList();
                    exposurePathSubstanceCollection.Add((path, substance, exposures));
                }
            }
            return exposurePathSubstanceCollection;
        }
    }
}
