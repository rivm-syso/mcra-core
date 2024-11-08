using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks {
    public sealed class AopKeyEventRelationshipRecord {
        public string CodeUpstreamKeyEvent { get; set; }
        public string CodeDownstreamKeyEvent { get; set; }

        /// <summary>
        /// A flag stating whether this KER is an indirect KER.
        /// E.g., when there is a path A -> B -> C, then A -> C is
        /// an indirect KER.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public bool IsIndirect { get; set; }

        /// <summary>
        /// A flag stating whether this KER is causing a cycle in the
        /// AOP network.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public bool IsCyclic { get; set; }
    }
}
