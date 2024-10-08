namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceUpperPieChartCreator(
        RiskContributionsBySubstanceUpperSection section,
        bool isUncertainty
    ) : RiskContributionsBySubstancePieChartCreatorBase(section, isUncertainty) {

        public override string Title => $"Substance contributions to the upper distribution.";

    }
}
