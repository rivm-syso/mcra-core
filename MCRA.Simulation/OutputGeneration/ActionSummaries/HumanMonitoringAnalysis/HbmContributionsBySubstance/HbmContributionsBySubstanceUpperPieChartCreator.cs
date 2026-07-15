namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmContributionsBySubstanceUpperPieChartCreator
        : HbmRiskDriverContributionsPieChartCreatorBase<SubstanceContributorKey, HbmContributionBySubstanceRecord> {

        private readonly double _upperPercentage;

        public HbmContributionsBySubstanceUpperPieChartCreator(
            string sectionId,
            List<HbmContributionBySubstanceRecord> records,
            double upperPercentage,
            bool isUncertainty
        ) : base(sectionId, records, isUncertainty) {
            Width = 500;
            Height = 350;
            _upperPercentage = upperPercentage;
        }

        public override string Title => $"Contribution of substances to the upper tail {_upperPercentage:F1}% of the distribution.";
    }
}
