using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureTotalDistributionSection : CoExposureDistributionSectionBase {

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> selectedSubstances,
            ExposureType exposureType
        ) {
            if (exposureType == ExposureType.Acute) {
                SummarizeAcute(dietaryIndividualDayIntakes, selectedSubstances);
            } else {
                SummarizeChronic(dietaryIndividualDayIntakes, selectedSubstances);
            }
        }
    }
}
