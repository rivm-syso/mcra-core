using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctions {

    public interface IExposureResponseFunctionModel {
        ExposureResponseFunction ExposureResponseFunction { get; set; }

        double Compute(double x);

        void ResampleModelParameters(IRandom random);
    }
}
