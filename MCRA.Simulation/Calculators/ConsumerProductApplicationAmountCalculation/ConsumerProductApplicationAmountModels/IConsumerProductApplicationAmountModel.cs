using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumerProductApplicationAmountCalculation {
    public interface IConsumerProductApplicationAmountModel {
        public ConsumerProductApplicationAmountSGs ApplicationAmount { get; }
        public double Draw(IRandom random, double? age, GenderType gender);
        public void CalculateParameters();
    }
}
