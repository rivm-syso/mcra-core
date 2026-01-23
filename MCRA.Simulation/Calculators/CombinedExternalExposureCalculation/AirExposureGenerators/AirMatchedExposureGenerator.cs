using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators {
    public class AirMatchedExposureGenerator : AirExposureGenerator {

        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<AirIndividualExposure> airIndividualExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var selected = airIndividualExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDays.First().SimulatedIndividual.Id);
            if (selected == null) {
                var msg = $"Failed to find matching air exposure for individual [{individualDays.First().SimulatedIndividual.Code}].";
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

