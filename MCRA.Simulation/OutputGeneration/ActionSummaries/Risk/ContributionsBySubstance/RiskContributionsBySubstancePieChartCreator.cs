﻿namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstancePieChartCreator(
        RiskContributionsBySubstanceSectionBase section,
        bool isUncertainty
    ) : RiskContributionsBySubstancePieChartCreatorBase(section, isUncertainty) {

        public override string Title => $"Substance contributions to the total distribution.";

    }
}
