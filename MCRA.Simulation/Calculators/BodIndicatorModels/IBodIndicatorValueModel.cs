using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {

    public interface IBodIndicatorValueModel {
        BurdenOfDisease BurdenOfDisease { get; set; }
        void CalculateParameters();
        void ResampleModelParameters(IRandom random);
        double GetBodIndicatorValue();
    }
}
