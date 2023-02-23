namespace MCRA.General.DoseResponseModels {

    public enum RiskType {
        Undefined = -1,
        Ed50 = 1,
        AdditionalRisk = 2,
        ExtraRisk = 3,
    }

    public interface IDoseResponseModelFunction {
        DoseResponseModelType Type { get; }
        void Init(IDictionary<string, double> parameters);
        void Init(IDictionary<string, double> parameters, double bmd, double bmr);
        double Calculate(double dose);
        double ComputeBmr(double ced, double ces, RiskType riskType);
        void DeriveParameterB(double bmd, double bmr);
        List<double> Calculate(List<double> doses);
        Dictionary<string, double> ExportParameters();
    }
}
