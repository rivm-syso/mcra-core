using MCRA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public interface IAdjustmentFactorModelFactorySettings {
        AdjustmentFactorDistributionMethod AdjustmentFactorDistributionMethod { get; }
        double ParameterA { get; }
        double ParameterB { get; }
        double ParameterC { get; }
        double ParameterD { get; }
    }
}
