using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public interface IFoodExtrapolationCandidatesCalculatorSettings {
        int ThresholdForExtrapolation { get; }
        bool ConsiderAuthorisationsForExtrapolations { get; }
        bool ConsiderMrlForExtrapolations { get; }

    }
}
