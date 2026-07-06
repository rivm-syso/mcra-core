namespace MCRA.Simulation.OutputGeneration {
    public class HbmContributionsBySubstanceUpperStackedBarChartCreator
        : HbmStackedBarChartCreatorBase<HbmSubstanceContributorKey, HbmContributionBySubstanceRecord> {

        public HbmContributionsBySubstanceUpperStackedBarChartCreator(
            string sectionId,
            List<HbmContributionBySubstanceRecord> records,
            bool isUncertainty
        ) : base(sectionId, records, isUncertainty) {
            Width = 500;
            Height = 350;
        }

        public override string Title => "Contribution by substances for the upper distribution.";
    }
}
