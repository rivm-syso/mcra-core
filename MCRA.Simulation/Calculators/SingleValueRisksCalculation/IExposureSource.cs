using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public interface IExposureSource {
        string Code { get; }
        string Name { get; }
        ExposureRouteType Route { get; set; }
    }
}
