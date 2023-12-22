using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Base class for a summary of the simulation
    /// </summary>
    public abstract class ActionSummarySectionBase : SummarySection {

        [Display(AutoGenerateField = false)]
        public override bool SaveTemporaryData => true;

    }
}
