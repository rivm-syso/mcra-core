using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CoExposureUpperDistributionSection : CoExposureDistributionSectionBase {

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            var upperPercentage = percentageForUpperTail;
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            var upperIntakes = upperIntakeCalculator
                .GetUpperTargetIndividualExposures(
                    aggregateExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    upperPercentage,
                    externalExposureUnit,
                    targetUnit
                );
            Summarize(upperIntakes, substances, targetUnit);
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
