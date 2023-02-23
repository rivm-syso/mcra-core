namespace MCRA.General.DoseResponseModels {

    public abstract class DoseResponseModelFunctionBase : IDoseResponseModelFunction {

        public abstract DoseResponseModelType Type { get; }

        public abstract double Calculate(double dose);

        public List<double> Calculate(List<double> doses) {
            return doses.Select(dose => Calculate(dose)).ToList();
        }

        public abstract double ComputeBmr(double ced, double ces, RiskType riskType);

        public abstract void Init(IDictionary<string, double> parameters);

        public virtual void Init(IDictionary<string, double> parameters, double bmd, double bmr) {
            Init(parameters);
            DeriveParameterB(bmd, bmr);
        }

        public abstract Dictionary<string, double> ExportParameters();

        protected double parseParameter(IDictionary<string, double> parameters, string name, double defaultIfNull = double.NaN) {
            if (parameters.ContainsKey(name)) {
                return parameters[name];
            } else {
                return defaultIfNull;
            }
            throw new Exception("This parameter is not available");
        }

        public abstract void DeriveParameterB(double bmd, double bmr);

    }
}
