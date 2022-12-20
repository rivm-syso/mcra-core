using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks {
    public sealed class AopKeyEventRecord {
        public string Code { get; set; }
        public string Name { get; set; }
        public BiologicalOrganisationType BiologicalOrganisationType { get; set; }
    }
}
