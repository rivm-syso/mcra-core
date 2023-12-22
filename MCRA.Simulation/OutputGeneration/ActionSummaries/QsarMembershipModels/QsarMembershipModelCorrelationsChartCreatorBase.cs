namespace MCRA.Simulation.OutputGeneration {
    public abstract class QsarMembershipModelCorrelationsChartCreatorBase : ReportCorrelationsChartCreatorBase {

        protected QsarMembershipModelCorrelationsSection _section;

        public QsarMembershipModelCorrelationsChartCreatorBase(QsarMembershipModelCorrelationsSection section) {
            _section = section;
            Height = 200 + Math.Max(_section.ModelNames.Count * CellSize, 100);
            Width = 200 + Math.Max(_section.ModelNames.Count * CellSize, 100);
        }
    }
}
