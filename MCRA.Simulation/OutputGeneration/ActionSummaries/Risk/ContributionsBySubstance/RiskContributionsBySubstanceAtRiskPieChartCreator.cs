namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceAtRiskPieChartCreator(
        RiskContributionsBySubstanceSectionBase section,
        bool isUncertainty
    ) : RiskContributionsBySubstancePieChartCreatorBase(section, isUncertainty) {

        public override string Title => $"Substance contributions to the upper distribution exceeding the risk threshold.";

    }
}
