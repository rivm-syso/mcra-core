using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.DietExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DietExposureGenerator {
    public abstract class DietExposureGenerator {

        /// <summary>
        /// Generates diet individual day exposures.
        /// </summary>
        public ICollection<DietIndividualDayExposure> GenerateDietIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            int seed,
            CancellationToken cancelToken
        ) {
            var dietIndividualExposures = individualDays
                .GroupBy(r => r.SimulatedIndividual.Id, (key, g) => g.First())
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var dietIndividualDayExposure = createDietIndividualExposure(
                        individualDay,
                        dietaryIndividualDayIntakes,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                    );
                    return dietIndividualDayExposure;
                })
                .ToList();

            // Check if success
            if (dietIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any dietary exposure to a dietary exposure");
            }
            return dietIndividualExposures;
        }

        protected abstract DietIndividualDayExposure createDietIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DietaryIndividualDayIntake> dietIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
