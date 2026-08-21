namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceAtRiskPieChartCreator(
        string sectionId,
        List<RiskContributionBySubstanceRecord> records,
        bool isUncertainty
    ) : RiskContributionsBySubstancePieChartCreatorBase(sectionId, records, isUncertainty) {

        public override string Title => $"Substance contributions to the upper distribution exceeding the risk threshold.";
    }
}
