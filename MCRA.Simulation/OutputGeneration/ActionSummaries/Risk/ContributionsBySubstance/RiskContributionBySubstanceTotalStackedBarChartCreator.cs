namespace MCRA.Simulation.OutputGeneration {
    public class RiskContributionBySubstanceTotalStackedBarChartCreator
        : RiskStackedBarChartCreatorBase<SubstanceContributorKey, RiskContributionBySubstanceRecord> {

        public RiskContributionBySubstanceTotalStackedBarChartCreator(
            string sectionId,
            List<RiskContributionBySubstanceRecord> records,
            bool isUncertainty
        ) : base(sectionId, records, isUncertainty) {
            Width = 500;
            Height = 350;
        }

        public override string Title => "Contribution by substances for the total distribution.";
    }
}
