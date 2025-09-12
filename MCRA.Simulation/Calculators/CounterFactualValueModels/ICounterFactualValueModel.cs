using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public interface ICounterFactualValueModelParametrisation {
        double Factor { get; set; }
    }

    public interface ICounterFactualValueModel {

        ExposureResponseFunction ExposureResponseFunction { get; }

        List<ICounterFactualValueModelParametrisation> GetParametrisations();

        void CalculateParameters();

        void ResampleModelParameters(IRandom random);

        double GetCounterFactualValue();

    }
}
