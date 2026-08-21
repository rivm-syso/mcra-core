namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstancePieChartCreator(
        string sectionId,
        List<RiskContributionBySubstanceRecord> records,
        bool isUncertainty
    ) : RiskContributionsBySubstancePieChartCreatorBase(sectionId, records, isUncertainty) {
        public override string Title => $"Substance contributions to the total distribution.";
    }
}
