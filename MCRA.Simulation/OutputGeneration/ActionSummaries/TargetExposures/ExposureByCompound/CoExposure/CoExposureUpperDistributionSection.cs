using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureUpperDistributionSection : CoExposureDistributionSectionBase {

        public void Summarize(
            ICollection<ITargetIndividualExposure> targetExposures,
            ICollection<ITargetIndividualDayExposure> targetDayExposures,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperPercentage = percentageForUpperTail;
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            if (targetExposures != null) {
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualExposures(targetExposures, relativePotencyFactors, membershipProbabilities, upperPercentage, isPerPerson);
                Summarize(upperIntakes, selectedSubstances);
            } else {
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualDayExposures(targetDayExposures, relativePotencyFactors, membershipProbabilities, upperPercentage, isPerPerson);
                Summarize(upperIntakes, selectedSubstances);
            }
        }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
            if (exposureType == ExposureType.Acute) {
                SummarizeAcute(upperIntakes, selectedSubstances);
            } else {
                SummarizeChronic(upperIntakes, selectedSubstances);
            }
        }
    }
}
