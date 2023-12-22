using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class MolecularDockingModelCorrelationsChartCreatorBase : ReportCorrelationsChartCreatorBase {

        protected MolecularDockingModelCorrelationsSummarySection _section;

        public MolecularDockingModelCorrelationsChartCreatorBase(MolecularDockingModelCorrelationsSummarySection section) {
            _section = section;
            Height = 200 + Math.Max(_section.ModelNames.Count * CellSize, 100);
            Width = 200 + Math.Max(_section.ModelNames.Count * CellSize, 100);
        }
    }
}
