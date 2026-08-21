namespace MCRA.Simulation.OutputGeneration {
    public class IndividualContributionsTotalStackedBarChartCreator
        : RiskStackedBarChartCreatorBase<SubstanceContributorKey, RiskContributionBySubstanceRecord> {

        public IndividualContributionsTotalStackedBarChartCreator(
            string sectionId,
            List<RiskContributionBySubstanceRecord> records,
            bool isUncertainty
        ) : base(sectionId, records, isUncertainty) {
            Width = 500;
            Height = 350;
        }

        public override string Title => "Contribution to risks for individuals.";
    }
}
