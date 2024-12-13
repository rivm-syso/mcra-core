using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureDistributionSection : SummarySection {
        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes
        /// 1) Percentiles
        /// 2) Percentages
        /// 3) Percentiles for a grid of 100 values
        /// </summary>
        /// <param name="header"></param>
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            SectionHeader header,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var externalExposures = externalIndividualDayExposures.Select(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = externalIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var subHeader = header.GetSubSectionHeader<ExternalTotalExposureDistributionSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as ExternalTotalExposureDistributionSection;
                section.SummarizeUncertainty(externalIndividualDayExposures, relativePotencyFactors, membershipProbabilities, isPerPerson);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as IntakePercentileSection;
                section.SummarizeUncertainty(externalExposures, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as IntakePercentageSection;
                section.SummarizeUncertainty(externalExposures, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}
