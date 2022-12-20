using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks {
    public sealed class AopNetworkLayerRecord {

        public ICollection<AopKeyEventRecord> KeyEvents { get; set; }

        public AopNetworkLayerRecord() { }

        public int BiologicalOrganisationLevel { get; set; }
    }
}
