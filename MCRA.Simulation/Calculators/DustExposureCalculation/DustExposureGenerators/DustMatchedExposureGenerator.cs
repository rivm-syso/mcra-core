using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public class DustMatchedExposureGenerator : DustExposureGenerator {

        protected override List<DustIndividualDayExposure> createDustIndividualExposure(
            IIndividualDay individual,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var dustIndividualExposures = dustIndividualDayExposures
                .Where(r => r.SimulatedIndividualDayId == individual.SimulatedIndividualDayId)
                .ToList();

            return dustIndividualExposures;
        }
    }
}

