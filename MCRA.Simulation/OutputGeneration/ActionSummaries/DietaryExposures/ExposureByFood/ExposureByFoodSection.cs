using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureByFoodSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public void Summarize(
            SectionHeader header,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Food> allFoods,
            ICollection<Food> foodsAsEaten,
            ICollection<Food> foodsAsMeasured,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isTotalDietStudy,
            bool useReadAcrossFoodTranslations,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            SectionHeader subHeader;
            var order = 0;

            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);

            if (dietaryIndividualDayIntakes != null) {
                var section = new TotalDistributionFoodAsMeasuredSection() { ProgressState = ProgressState };
                subHeader = header.AddSubSectionHeaderFor(section, "As measured total distribution", order++);
                section.Summarize(
                    allFoods,
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    foodsAsMeasured,
                    exposureType,
                    lowerPercentage,
                    upperPercentage,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }

            if (dietaryIndividualDayIntakes != null) {
                var section = new UpperDistributionFoodAsMeasuredSection() { ProgressState = ProgressState };
                subHeader = header.AddSubSectionHeaderFor(section, "As measured upper tail", order++);
                section.Summarize( 
                    allFoods,
                    dietaryIndividualDayIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    foodsAsMeasured,
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

            if (dietaryIndividualDayIntakes != null) {
                var section = new TotalDistributionFoodAsEatenSection() { ProgressState = ProgressState };
                subHeader = header.AddSubSectionHeaderFor(section, "As eaten total distribution", order++);
                section.Summarize(
                    dietaryIndividualDayIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities,
                    exposureType,
                    lowerPercentage,
                    upperPercentage,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }

            if (dietaryIndividualDayIntakes != null) {
                var section = new UpperDistributionFoodAsEatenSection() { ProgressState = ProgressState };
                subHeader = header.AddSubSectionHeaderFor(section, "As eaten upper tail", order++);
                section.Summarize(
                    dietaryIndividualDayIntakes, 
                    relativePotencyFactors, 
                    membershipProbabilities, 
                    foodsAsEaten,
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

            if (isTotalDietStudy && useReadAcrossFoodTranslations) {
                if (dietaryIndividualDayIntakes != null) {
                    var section = new TotalDistributionTDSFoodAsMeasuredSection() { ProgressState = ProgressState };
                    subHeader = header.AddSubSectionHeaderFor(section, "Contribution of TDS vs. Read Across, total distribution", order++);
                    section.Summarize(
                        dietaryIndividualDayIntakes,
                        substances,
                        relativePotencyFactors,
                        membershipProbabilities,
                        exposureType,
                        lowerPercentage,
                        upperPercentage,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }

                if (dietaryIndividualDayIntakes != null) {
                    var section = new UpperDistributionTDSFoodAsMeasuredSection() { ProgressState = ProgressState };
                    subHeader = header.AddSubSectionHeaderFor(section, "Contribution of TDS vs. Read Across, upper distribution", order++);
                    section.Summarize(
                        dietaryIndividualDayIntakes,
                        substances,
                        relativePotencyFactors,
                        membershipProbabilities,
                        exposureType,
                        lowerPercentage,
                        upperPercentage,
                        percentageForUpperTail,
                        isPerPerson
                        );
                    subHeader.SaveSummarySection(section);
                }
            }
        }


        public void SummarizeUncertain(
            SectionHeader header,
            ICollection<Food> allFoods,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);

            if (dietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<TotalDistributionFoodAsMeasuredSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as TotalDistributionFoodAsMeasuredSection;
                    section.SummarizeUncertainty(
                        allFoods,
                        dietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (dietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<UpperDistributionFoodAsMeasuredSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as UpperDistributionFoodAsMeasuredSection;
                    section.SummarizeUncertainty(
                        allFoods,
                        dietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (dietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<TotalDistributionFoodAsEatenSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as TotalDistributionFoodAsEatenSection;
                    section.SummarizeUncertainty(
                        dietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
            if (dietaryIndividualDayIntakes != null) {
                var subHeader = header.GetSubSectionHeader<UpperDistributionFoodAsEatenSection>();
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as UpperDistributionFoodAsEatenSection;
                    section.SummarizeUncertainty(
                        dietaryIndividualDayIntakes,
                        relativePotencyFactors,
                        membershipProbabilities,
                        exposureType,
                        isPerPerson
                    );
                    subHeader.SaveSummarySection(section);
                }
            }
        }
    }
}
