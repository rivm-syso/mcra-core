using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Simulation.OutputGeneration {
    public class HCSubgroupPlotRecord {
        public double Age { get; set; }
        public double HazardCharacterisationValue { get; set; }
        public List<double> UncertaintyValues { get; set; }
    }
}
