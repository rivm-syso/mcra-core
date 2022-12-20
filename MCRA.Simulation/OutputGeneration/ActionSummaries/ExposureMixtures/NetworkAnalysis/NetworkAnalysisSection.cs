using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NetworkAnalysisSection : SummarySection {

        public double[,] GlassoSelect { get; set; }
        public List<string> SubstanceCodes { get; set; }

        public void Summarize(
            double [,] glassoSelect,
            List<Compound> substances
        ) {
            GlassoSelect = glassoSelect;  
            SubstanceCodes = substances.Select(c => c.Code).ToList();   
        }
    }
}
