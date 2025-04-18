using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.AirExposureGenerators {
    public class AirMatchedExposureGenerator : AirExposureGenerator {

        protected override List<AirIndividualDayExposure> createAirIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var airIndividualExposure = airIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDays.First().SimulatedIndividual.Id);
            if (airIndividualExposure == null) {
                var msg = $"Failed to find matching air exposure for individual [{individualDays.First().SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var results = new List<AirIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = airIndividualExposure.Clone(individualDay);
                results.Add(result);
            }
            return results;
        }
    }
}

