using MCRA.Data.Compiled.Objects;
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
                    var kineticConversionFactor = kineticConversionFactors[(path.Route, substance)];
                    var exposures = externalIndividualExposures
                        .Select(c => (
                            SimulatedIndividual: c.SimulatedIndividual,
                            Exposure: c.GetExposure(path, substance, isPerPerson) * kineticConversionFactor
                        ))
                        .ToList();
                    exposurePathSubstanceCollection.Add((path, substance, exposures));
                }
            }
            return exposurePathSubstanceCollection;
        }
    }
}
