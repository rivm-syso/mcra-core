using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.DietaryExposures {
    public sealed class SubstanceDetailSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public string Code { get; set; }
        public string Name { get; set; }

        public void Summarize(
            SectionHeader header,
            double[] selectedPercentiles,
            ExposureMethod exposureMethod,
            double[] selectedExposureLevels,
            Compound referenceSubstance,
            Compound substance,
            List<double> intakes,
            List<double> weights
        ) {
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                intakes,
                exposureMethod,
                selectedExposureLevels
            );
            var totalDistributionSection = new OIMDistributionSection(true, false);
            var subHeader = header.AddSubSectionHeaderFor(totalDistributionSection, "Graph total", 1);
            totalDistributionSection.Summarize(intakes, weights);
            subHeader.SaveSummarySection(totalDistributionSection);

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 2);
            percentileSection.Summarize(intakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 3);
            percentageSection.Summarize(intakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);

            Code = substance.Code;
            Name = substance.Name;
        }

        public void SummarizeUncertainty(
            SectionHeader header,
            List<double> intakes,
            List<double> weights,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var subHeader = header.GetSubSectionHeader<OIMDistributionSection>();
            if (subHeader != null) {
                var totalDistributionSection = subHeader.GetSummarySection() as OIMDistributionSection;
                totalDistributionSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
                subHeader.SaveSummarySection(totalDistributionSection);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
                subHeader.SaveSummarySection(percentileSection);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
                subHeader.SaveSummarySection(percentageSection);
            }
        }
    }
}
