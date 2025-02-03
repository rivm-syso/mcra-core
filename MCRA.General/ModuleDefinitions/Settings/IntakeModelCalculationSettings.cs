using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.General.ModuleDefinitions.Settings {
    public class IntakeModelCalculationSettings : IIntakeModelCalculationSettings {
        private readonly CovariateModelType _covariateModelType;
        private readonly FunctionType _functionType;
        private readonly double _testingLevel;
        private readonly TestingMethodType _testingMethod;
        private readonly int _minDegreesOfFreedom;
        private readonly int _maxDegreesOfFreedom;

        public IntakeModelCalculationSettings(
            CovariateModelType covariateModelType = default,
            FunctionType functionType = default,
            double testingLevel = 0.05,
            TestingMethodType testingMethod = default,
            int minDegreesOfFreedom = 0,
            int maxDegreesOfFreedom = 4
        ) {
            _covariateModelType = covariateModelType;
            _functionType = functionType;
            _testingLevel = testingLevel;
            _testingMethod = testingMethod;
            _minDegreesOfFreedom = minDegreesOfFreedom;
            _maxDegreesOfFreedom = maxDegreesOfFreedom;
        }

        public CovariateModelType CovariateModelType => _covariateModelType;

        public FunctionType FunctionType => IsCovariateModel ? _functionType : FunctionType.Polynomial;

        public double TestingLevel => IsCovariateModel ? _testingLevel : 0.05;

        public TestingMethodType TestingMethod => IsCovariateModel ? _testingMethod : TestingMethodType.Backward;

        public int MinDegreesOfFreedom => IsCovariateModel ? _minDegreesOfFreedom : 0;

        public int MaxDegreesOfFreedom => IsCovariateModel ? _maxDegreesOfFreedom : 4;

        private bool IsCovariateModel =>
            _covariateModelType != CovariateModelType.Constant &&
            _covariateModelType != CovariateModelType.Cofactor;
    }
}
