namespace MCRA.Simulation.OutputGeneration {
    public abstract class ActiveSubstanceModelCorrelationsChartCreatorBase : ReportCorrelationsChartCreatorBase {

        protected ActiveSubstanceModelCorrelationsSection _section;

        public ActiveSubstanceModelCorrelationsChartCreatorBase(ActiveSubstanceModelCorrelationsSection section) {
            _section = section;
            Height = 200 + Math.Max(_section.ModelNames.Count * CellSize, 100);
            Width = 200 + Math.Max(_section.ModelNames.Count * CellSize, 100);
        }
    }
}
