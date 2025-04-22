using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DustExposureGenerators {
    public class DustMatchedExposureGenerator : DustExposureGenerator {

        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var selected = dustIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDays.First().SimulatedIndividual.Id);
            if (selected == null) {
                var msg = $"Failed to find matching dust exposure for individual [{individualDays.First().SimulatedIndividual.Code}].";
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

