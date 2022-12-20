using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureTotalDistributionSection : CoExposureDistributionSectionBase {
        public void Summarize(
            ICollection<ITargetIndividualExposure> targetIndividualExposures,
            ICollection<ITargetIndividualDayExposure> targetIndividualDayExposures,
            ICollection<Compound> selectedSubstances
        ) {
            if (targetIndividualExposures != null) {
                Summarize(targetIndividualExposures, selectedSubstances);
            } else {
                Summarize(targetIndividualDayExposures, selectedSubstances);
            }
        }

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
