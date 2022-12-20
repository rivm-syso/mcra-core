using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AnalyticalMethodSummaryRecord {

        public string AnalyticalMethodName { get; set; }

        public string AnalyticalMethodCode { get; set; }

        public int NumberOfSamples { get; set; }

        public List<string> SubstanceNames { get; set; }

        public List<string> SubstanceCodes { get; set; }

        public List<double> Lods { get; set; }
        public List<double> Loqs { get; set; }

        public List<string> ConcentrationUnits { get; set; }

    }
}
