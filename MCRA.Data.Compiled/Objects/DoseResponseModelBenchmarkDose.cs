﻿namespace MCRA.Data.Compiled.Objects {
    public sealed class DoseResponseModelBenchmarkDose {

        private const string _sep = "\a";

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
        public ICollection<DoseResponseModelBenchmarkDoseUncertain> DoseResponseModelBenchmarkDoseUncertains { get; set; } = [];

        public string Key => string.Join(_sep, [IdDoseResponseModel, Substance.Code, CovariateLevel ?? string.Empty]);

        public DoseResponseModelBenchmarkDose CreateBootstrapRecord(
            double drawnBmd,
            double drawnRpf
        ) {
            var drmBMD = new DoseResponseModelBenchmarkDose() {
                IdDoseResponseModel = IdDoseResponseModel,
                Substance = Substance,
                CovariateLevel = CovariateLevel,
                BenchmarkDose = drawnBmd,
                BenchmarkDoseLower = BenchmarkDoseLower,
                BenchmarkDoseUpper = BenchmarkDoseUpper,
                BenchmarkResponse = BenchmarkResponse,
                Rpf = drawnRpf,
                RpfLower = RpfLower,
                RpfUpper = RpfUpper,
                ModelParameterValues = ModelParameterValues,
            };
            return drmBMD;
        }
    }
}
