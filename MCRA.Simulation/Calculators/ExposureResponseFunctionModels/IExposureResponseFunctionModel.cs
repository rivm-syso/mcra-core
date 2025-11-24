using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.CounterFactualValueModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctions {

    public interface IExposureResponseFunctionModel {
        ExposureResponseFunction ExposureResponseFunction { get; set; }
        ICounterFactualValueModel CounterFactualValueModel { get; set; }
        double Compute(double x, bool useErfBins);
        void ResampleModelParameters(IRandom random);
    }
}
