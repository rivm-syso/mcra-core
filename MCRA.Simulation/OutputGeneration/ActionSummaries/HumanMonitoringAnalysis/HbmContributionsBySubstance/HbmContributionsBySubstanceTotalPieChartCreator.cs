namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmContributionsBySubstanceTotalPieChartCreator
        : HbmRiskDriverContributionsPieChartCreatorBase<SubstanceContributorKey, HbmContributionBySubstanceRecord> {

        public HbmContributionsBySubstanceTotalPieChartCreator(
            string sectionId,
            List<HbmContributionBySubstanceRecord> records,
            bool isUncertainty
        ) : base(sectionId, records, isUncertainty) {
            Width = 500;
            Height = 350;
        }

        public override string Title => "Contribution of substances to total distribution.";
    }
}
