using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.ChlorpyrifosKineticModelCalculation {
    public sealed class ChlorpyrifosKineticModelCalculator : PbpkModelCalculator {
        public ChlorpyrifosKineticModelCalculator(
          KineticModelInstance kineticModelInstance,
          Dictionary<ExposureRouteType, double> defaultAbsorptionFactors
      ) : base(kineticModelInstance, defaultAbsorptionFactors) {
        }

        protected override double getRelativeCompartmentWeight(KineticModelOutputDefinition parameter, Dictionary<string, double> parameters) {
            var factor = 1D;
            foreach (var scalingFactor in parameter.ScalingFactors) {
                factor *= parameters[scalingFactor];
            }
            return factor;
        }
    }
}
