using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {
    public interface IBodIndicatorModel {

        Population Population { get; }

        Effect Effect { get; }

        BodIndicator BodIndicator { get; }

        double GetBodIndicatorValue();
        
        void ResampleModelParameters(IRandom random);

    }
}