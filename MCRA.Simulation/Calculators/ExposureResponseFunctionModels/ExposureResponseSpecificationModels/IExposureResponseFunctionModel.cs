using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {

    public interface IExposureResponseFunctionModel {
        double Compute(double exposure, double counterFactualValue);
        public void Resample(IRandom random);
    }
}
