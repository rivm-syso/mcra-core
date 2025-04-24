using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DietExposureGenerator {

    public class DietUnmatchedCorrelatedExposureGenerator : DietExposureGenerator {

        /// <summary>
        /// Randomly pair air and dietary individuals
        /// (if the properties of the dietary individual match the properties of the air individual)
        /// </summary>
        protected override ExternalIndividualDayExposure generate(
            IIndividualDay individualDay,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            throw new NotImplementedException();
        }
    }
}
