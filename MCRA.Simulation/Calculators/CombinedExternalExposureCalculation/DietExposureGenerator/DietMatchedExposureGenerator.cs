using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DietExposureGenerator {
    public class DietMatchedExposureGenerator : DietExposureGenerator {

        protected override ExternalIndividualDayExposure generate(
            IIndividualDay individualDay,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var dietIndividualDayIntake = dietaryIndividualDayIntakes
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDay.SimulatedIndividual.Id);
            if (dietIndividualDayIntake == null) {
                var msg = $"Failed to find matching exposure for individual [{individualDay.SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                [new(ExposureSource.Diet, ExposureRoute.Oral)] = [.. dietIndividualDayIntake.GetTotalIntakesPerSubstance()]
            };
            var result = new ExternalIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day
            };
            return result;
        }
    }
}

