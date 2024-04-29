using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class AggregateIndividualExposureCollection : TargetExposure {
        public List<AggregateIndividualExposure> AggregateIndividualExposures { get; set; }
    }
}
