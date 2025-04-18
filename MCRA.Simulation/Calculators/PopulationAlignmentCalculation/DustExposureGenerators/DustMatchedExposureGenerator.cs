using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DustExposureGenerators {
    public class DustMatchedExposureGenerator : DustExposureGenerator {

        protected override List<DustIndividualDayExposure> createDustIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var dustIndividualExposure = dustIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDays.First().SimulatedIndividual.Id);
            if (dustIndividualExposure == null) {
                var msg = $"Failed to find matching dust exposure for individual [{individualDays.First().SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var results = new List<DustIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = dustIndividualExposure.Clone(individualDay);
                results.Add(result);
            }
            return results;
        }
    }
}

