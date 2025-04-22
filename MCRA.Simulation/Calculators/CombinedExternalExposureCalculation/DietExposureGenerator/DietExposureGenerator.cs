using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DietExposureGenerator {
    public abstract class DietExposureGenerator {

        /// <summary>
        /// Generates diet individual day exposures.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var dietIndividualExposures = individualDays
                .AsParallel()
                .Select(individualDay => generate(
                    individualDay,
                    dietaryIndividualDayIntakes,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                ))
                .Cast<IExternalIndividualDayExposure>()
                .ToList();

            // Check if success
            if (dietIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any dietary exposure to a dietary exposure.");
            }
            var dietExposureCollection = new ExternalExposureCollection {
                ExposureUnit = new ExposureUnitTriple(substanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                ExposureSource = ExposureSource.Diet,
                ExternalIndividualDayExposures = dietIndividualExposures
            };
            return dietExposureCollection;
        }

        protected abstract ExternalIndividualDayExposure generate(
            IIndividualDay individualDay,
            ICollection<DietaryIndividualDayIntake> dietIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
