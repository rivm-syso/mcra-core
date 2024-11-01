using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustMatchedExposureGenerator : DustExposureGenerator {

        protected override DustIndividualDayExposure createDustIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var dustIndividualExposures = dustIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividualDayId == individualDay.SimulatedIndividualDayId);
            if (dustIndividualExposures == null) {
                var msg = $"Failed to find matching dust exposure for individual [{individualDay.Individual.Code}] on day [{individualDay.Day}].";
                throw new Exception(msg);
            }
            var result = dustIndividualExposures.Clone();
            return dustIndividualExposures;
        }
    }
}

