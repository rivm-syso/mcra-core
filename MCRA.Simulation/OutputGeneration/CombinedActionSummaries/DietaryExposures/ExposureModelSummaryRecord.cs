using System;
using System.Collections.Generic;
using System.Linq;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureModelSummaryRecord {

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ExposureModelSummaryRecord() {
        }
    }
}
