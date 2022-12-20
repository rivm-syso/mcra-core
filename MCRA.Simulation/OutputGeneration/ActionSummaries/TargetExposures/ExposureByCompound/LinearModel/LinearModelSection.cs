using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public class LinearModelSection : SummarySection {

        public List<CompoundRecord> Records { get; set; }

        public void Summarize(List<Compound> compounds) {
            Records = compounds.Select(c => new CompoundRecord() {
                Name = c.Name,
                Code = c.Code,
            }).ToList();
        }
    }
}
