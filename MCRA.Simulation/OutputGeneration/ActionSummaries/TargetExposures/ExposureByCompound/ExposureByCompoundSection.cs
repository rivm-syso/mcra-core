using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureByCompoundSection : SummarySection {

        /// <summary>
        /// Exposure by substances
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="exposurePerCompoundRecords"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="substances"></param>
        /// <param name="exposureType"></param>
        /// <param name="lowerPercentage"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            SectionHeader header,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            Dictionary<Compound, List<ExposureRecord>> exposurePerCompoundRecords,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
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
            var order = 1;
            var indexOrder = new List<string>();
            if (dietaryIndividualDayIntakes != null) {
                var section = new TotalDistributionCompoundSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Total distribution", order++);
                section.Summarize(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureType,
                    lowerPercentage,
                    upperPercentage,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    isPerPerson
                );
                indexOrder = section.Records.Select(c => c.CompoundCode).ToList();
                subHeader.SaveSummarySection(section);
            }

            if (dietaryIndividualDayIntakes != null) {
                var section = new TargetExposuresBySubstanceSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Total distribution (boxplots)", order++);
                section.Summarize(
                    dietaryIndividualDayIntakes,
                    substances,
                    indexOrder,
                    exposureType,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }

            if (dietaryIndividualDayIntakes != null
                && ((relativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
            ) {
                var section = new UpperDistributionCompoundSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Upper tail distribution", order++);
                section.Summarize(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureType,
                    percentageForUpperTail,
                    lowerPercentage,
                    upperPercentage,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
            if (exposurePerCompoundRecords != null) {
                var section = new CompoundExposureDistributionsSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Exposure distributions", order++);
                section.Summarize(
                    exposurePerCompoundRecords,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Co-exposures by substances
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="substances"></param>
        /// <param name="exposureType"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            SectionHeader header,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            SectionHeader subHeader;
            var order = 0;
            if (dietaryIndividualDayIntakes != null) {
                var section = new CoExposureTotalDistributionSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Co-exposure total distribution", order++);
                section.Summarize(
                    dietaryIndividualDayIntakes,
                    substances,
                    exposureType
                );
                subHeader.SaveSummarySection(section);
            }

            if (dietaryIndividualDayIntakes != null
                && ((relativePotencyFactors?.Any() ?? false) || (substances?.Count == 1))
            ) {
                var section = new CoExposureUpperDistributionSection();
                subHeader = header.AddSubSectionHeaderFor(section, "Co-exposure upper tail", order++);
                section.Summarize(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureType,
                    percentageForUpperTail,
                    isPerPerson
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
