using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Compiled.Objects {
    public sealed class AdverseOutcomePathwayNetwork {
        public Effect AdverseOutcome { get; set; }
        public List<EffectRelationship> EffectRelations { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string RiskTypeString { get; set; }

        public AdverseOutcomePathwayNetwork() {
            EffectRelations = new List<EffectRelationship>();
        }
    }
}
