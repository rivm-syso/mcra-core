using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public interface ICounterFactualValueModel {
        ExposureResponseFunction ExposureResponseFunction { get; }

        void CalculateParameters();

        void ResampleModelParameters(IRandom random);

        double GetCounterFactualValue();
    }
}
