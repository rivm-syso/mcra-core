using MCRA.Utils.ProgressReporting;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OIMModelSection : ChronicIntakeModelSection {

        public override void Summarize(SectionHeader header, ProjectDto project, ActionData dataSource, UsualIntakeResults usualIntakeResults, ProgressState p) {
        }

        public override void SummarizeUncertainty(ProjectDto project, UsualIntakeResults usualIntakeResults, ProgressState p) {
        }
    }
}
