using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.SoilExposureGenerators {
    public class SoilMatchedExposureGenerator : SoilExposureGenerator {

        protected override List<SoilIndividualDayExposure> createSoilIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
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
            var results = new List<SoilIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = soilIndividualExposure.Clone(individualDay);
                results.Add(result);
            }
            return results;
        }
    }
}

