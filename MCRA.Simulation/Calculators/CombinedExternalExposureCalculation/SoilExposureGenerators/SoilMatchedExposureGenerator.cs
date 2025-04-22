using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {
    public class SoilMatchedExposureGenerator : SoilExposureGenerator {
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var soilIndividualExposure = soilIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDays.First().SimulatedIndividual.Id);
            if (soilIndividualExposure == null) {
                var msg = $"Failed to find matching soil exposure for individual [{individualDays.First().SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = createExternalIndividualDayExposure(individualDay, soilIndividualExposure);
                results.Add(result);
            }
            return results;
        }
    }
}

