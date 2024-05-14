using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureTotalDistributionSection : CoExposureDistributionSectionBase {
        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            Summarize(aggregateExposures, substances, targetUnit);
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
