using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NormalAmountsModelResidualSection : UncorrelatedModelResultsSection {
        public List<double> Residuals { get; set; }

        public void Summarize(AmountsModelSummary amountsModelSummary) {
            Residuals = ((NormalAmountsModelSummary)amountsModelSummary).Residuals.Standardize();
        }
    }
}
