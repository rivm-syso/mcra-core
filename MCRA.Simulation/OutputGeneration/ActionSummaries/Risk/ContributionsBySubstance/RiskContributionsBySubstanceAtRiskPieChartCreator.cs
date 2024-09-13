namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceAtRiskPieChartCreator(
        RiskContributionsBySubstanceSection section,
        bool isUncertainty
    ) : RiskContributionsBySubstancePieChartCreator(section, isUncertainty) {

        public override string Title => $"Subtance contributions to the upper distrbution exceeding the risk threshold.";

    }
}
