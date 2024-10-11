namespace MCRA.Data.Compiled.Objects {
    public sealed class DoseResponseModelBenchmarkDoseUncertain {
        private const string _sep = "\a";

        public string IdDoseResponseModel { get; set; }
        public string IdUncertaintySet { get; set; }
        public Compound Substance { get; set; }
        public string CovariateLevel { get; set; }
        public double BenchmarkDose { get; set; }
        public double Rpf { get; set; }

        public string Key => string.Join(_sep, [IdDoseResponseModel, Substance.Code, CovariateLevel ?? string.Empty]);
    }
}