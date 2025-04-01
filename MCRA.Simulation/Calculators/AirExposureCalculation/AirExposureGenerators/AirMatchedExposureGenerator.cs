using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.AirExposureCalculation {
    public class AirMatchedExposureGenerator : AirExposureGenerator {

        protected override AirIndividualDayExposure createAirIndividualExposure(
            IIndividualDay individualDay,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var airIndividualExposures = airIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDay.SimulatedIndividual.Id);
            if (airIndividualExposures == null) {
                var msg = $"Failed to find matching air exposure for individual [{individualDay.SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var result = airIndividualExposures.Clone();
            return airIndividualExposures;
        }
    }
}

