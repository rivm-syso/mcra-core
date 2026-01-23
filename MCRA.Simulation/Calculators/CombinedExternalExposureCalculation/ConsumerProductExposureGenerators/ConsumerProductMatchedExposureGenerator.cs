using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.ConsumerProductExposureGenerators {
    public class ConsumerProductMatchedExposureGenerator : ConsumerProductExposureGenerator {

        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<ConsumerProductIndividualExposure> cpIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var selected = cpIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDays.First().SimulatedIndividual.Id);
            if (selected == null) {
                var msg = $"Failed to find matching consumer product exposure for individual [{individualDays.First().SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = createExternalIndividualDayExposure(individualDay, selected);
                results.Add(result);
            }
            return results;
        }
    }
}
