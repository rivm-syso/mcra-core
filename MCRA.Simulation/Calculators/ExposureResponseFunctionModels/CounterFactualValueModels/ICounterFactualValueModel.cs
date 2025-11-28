using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels {

    public interface ICounterFactualValueModel {

        void ResampleModelParameters(IRandom random);

        double GetCounterFactualValue();
    }
}
