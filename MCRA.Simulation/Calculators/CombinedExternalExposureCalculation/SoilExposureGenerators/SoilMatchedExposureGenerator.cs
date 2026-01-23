using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {
    public class SoilMatchedExposureGenerator : SoilExposureGenerator {
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<SoilIndividualExposure> soilIndividualExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var soilIExposure = soilIndividualExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDays.First().SimulatedIndividual.Id);
            if (soilIExposure == null) {
                var msg = $"Failed to find matching soil exposure for individual [{individualDays.First().SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = createExternalIndividualDayExposure(individualDay, soilIExposure);
                results.Add(result);
            }
            return results;
        }
    }
}

