using MCRA.Utils.ProgressReporting;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ChronicIntakeModelSection : SummarySection {

        public abstract void Summarize(SectionHeader header, ProjectDto project, ActionData dataSource, UsualIntakeResults usualIntakeResults, ProgressState p);

        public abstract void SummarizeUncertainty(ProjectDto project, UsualIntakeResults usualIntakeResults, ProgressState p);
    }
}