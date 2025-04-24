using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureByFoodCompoundSection : SummarySection {

        public void Summarize(
            SectionHeader header,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Food> modelledFoods,
            ICollection<Compound> substances,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            SectionHeader subHeader;
            if (dietaryIndividualDayIntakes != null) {
                var section = new TotalDistributionFoodCompoundSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Risk drivers total distribution", 1);
                section.Summarize(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    modelledFoods,
                    substances,
                    exposureType,
                    lowerPercentage,
                    upperPercentage,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }

            if (dietaryIndividualDayIntakes != null && relativePotencyFactors != null) {
                var section = new UpperDistributionFoodCompoundSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Risk drivers upper tail", 2);
                section.Summarize(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    modelledFoods,
                    substances,
                    exposureType,
                    lowerPercentage,
                    upperPercentage,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    percentageForUpperTail,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
