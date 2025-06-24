using MCRA.General;

namespace MCRA.Simulation.Calculators.ConsumerProductApplicationAmountCalculation {
    public class ConsumerProductApplicationAmountCalculatorFactory {

        public static IConsumerProductApplicationAmountModel Create(
            ConsumerProductApplicationAmountSGs applicationAmount
        ) {
            IConsumerProductApplicationAmountModel model = null;
            switch (applicationAmount.DistributionType) {
                case ApplicationAmountDistributionType.Constant:
                    model = new ConsumerProductApplicationAmountConstantModel(applicationAmount);
                    break;
                case ApplicationAmountDistributionType.LogNormal:
                    model = new ConsumerProductApplicationAmountLogNormalModel(applicationAmount);
                    break;
            }
            model.CalculateParameters();
            return model;
        }
    }
}
