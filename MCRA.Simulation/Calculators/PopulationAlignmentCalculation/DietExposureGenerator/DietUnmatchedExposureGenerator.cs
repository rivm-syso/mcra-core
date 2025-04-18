using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.DietExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DietExposureGenerator {

    public class DietUnmatchedExposureGenerator : DietExposureGenerator {

        /// <summary>
        /// Randomly pair diet and reference individuals
        /// </summary>
        protected override DietIndividualDayExposure createDietIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, dietaryIndividualDayIntakes.Count);
            var individualdayIntake = dietaryIndividualDayIntakes.ElementAt(ix);

            var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                [new(ExposureSource.Diet, ExposureRoute.Oral)] = [.. individualdayIntake.GetTotalIntakesPerSubstance()]
            };

            var result = new DietIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day
            };
            return result;
        }
    }
}
