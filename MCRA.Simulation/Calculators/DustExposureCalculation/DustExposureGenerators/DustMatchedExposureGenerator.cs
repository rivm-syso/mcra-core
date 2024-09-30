using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustMatchedExposureGenerator : DustExposureGenerator {

        protected override List<DustIndividualDayExposure> createDustIndividualExposure(
            IIndividualDay individual,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var dustIndividualExposures = dustIndividualDayExposures
                .Where(r => r.SimulatedIndividualId == individual.SimulatedIndividualId)
                .ToList();

            return dustIndividualExposures;
        }
    }
}

