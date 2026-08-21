namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceUpperPieChartCreator(
        string sectionId,
        List<RiskContributionBySubstanceRecord> records,
        bool isUncertainty
    ) : RiskContributionsBySubstancePieChartCreatorBase(sectionId, records, isUncertainty) {

        public override string Title => $"Substance contributions to the upper distribution.";

    }
}
