namespace MCRA.Data.Compiled.Objects {
    public sealed class DoseResponseModelBenchmarkDose {

        private const string _sep = "\a";

        public DoseResponseModelBenchmarkDose() {
            DoseResponseModelBenchmarkDoseUncertains = new HashSet<DoseResponseModelBenchmarkDoseUncertain>();
        }

        public string IdDoseResponseModel { get; set; }
        public Compound Substance { get; set; }
        public string CovariateLevel { get; set; }
        public double BenchmarkDose { get; set; }
        public double BenchmarkDoseLower { get; set; }
        public double BenchmarkDoseUpper { get; set; }
        public double BenchmarkResponse { get; set; }
        public double Rpf { get; set; }
        public double RpfLower { get; set; }
        public double RpfUpper { get; set; }
        public string ModelParameterValues { get; set; }
        public ICollection<DoseResponseModelBenchmarkDoseUncertain> DoseResponseModelBenchmarkDoseUncertains { get; set; }

        public string Key {
            get {
                return string.Join(_sep, new[] { IdDoseResponseModel, Substance.Code, CovariateLevel ?? string.Empty });
            }
        }
        
        public DoseResponseModelBenchmarkDose CreateBootstrapRecord(double drawnBmd, double drawnRpf) {
            var drmBMD = new DoseResponseModelBenchmarkDose() {
                IdDoseResponseModel = IdDoseResponseModel,
                Substance = Substance,
                CovariateLevel = CovariateLevel,
                BenchmarkDose = drawnBmd,
                BenchmarkDoseLower = double.NaN,
                BenchmarkDoseUpper = double.NaN,
                BenchmarkResponse = BenchmarkResponse,
                Rpf = drawnRpf,
                RpfLower = double.NaN,
                RpfUpper = double.NaN,
                ModelParameterValues = ModelParameterValues,
            };
            return drmBMD;
        }
    }
}