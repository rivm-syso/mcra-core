namespace MCRA.General.Action.Settings {

    public class AmountModelSettings {
        public virtual CovariateModelType CovariateModelType { get; set; } = CovariateModelType.Constant;

        public virtual FunctionType FunctionType { get; set; }

        public virtual double TestingLevel { get; set; } = 0.05D;

        public virtual TestingMethodType TestingMethod { get; set; } = TestingMethodType.Backward;

        public virtual int MinDegreesOfFreedom { get; set; } = 0;

        public virtual int MaxDegreesOfFreedom { get; set; } = 4;
    }
}
