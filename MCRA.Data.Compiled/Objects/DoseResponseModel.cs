using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DoseResponseModel {

        public string IdDoseResponseModel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IdExperiment { get; set; }
        public DoseResponseModelType DoseResponseModelType { get; set; }
        public List<Compound> Substances { get; set; }
        public Response Response { get; set; }
        public List<string> Covariates { get; set; }
        public string BenchmarkResponseTypeString { get; set; }
        public double CriticalEffectSize { get; set; }
        public string ModelEquation { get; set; }
        public string ModelParameterValues { get; set; }
        public double? LogLikelihood { get; set; }
        public string DoseUnitString { get; set; }
        public string ExceptionMessage { get; set; }
        public string ProastVersion { get; set; }

        public Dictionary<string, DoseResponseModelBenchmarkDose> DoseResponseModelBenchmarkDoses { get; set; }

        public List<string> GetCovariateLevels() {
            return DoseResponseModelBenchmarkDoses?.Values?.Select(r => r.CovariateLevel).Distinct().ToList();
        }

        public int? GetNumberOfBootstraps() {
            return DoseResponseModelBenchmarkDoses?.Values
                .SelectMany(r => r.DoseResponseModelBenchmarkDoseUncertains)
                .GroupBy(r => r.IdUncertaintySet)
                .Count();
        }

        public DoseUnit DoseUnit {
            get {
                return DoseUnitConverter.FromString(this.DoseUnitString, DoseUnit.mgPerKgBWPerDay);
            }
        }

        public BenchmarkResponseType BenchmarkResponseType {
            get {
                if (!string.IsNullOrEmpty(BenchmarkResponseTypeString)) {
                    return BenchmarkResponseTypeConverter.FromString(BenchmarkResponseTypeString);
                } else {
                    return BenchmarkResponseType.Undefined;
                }
            }
            set {
                BenchmarkResponseTypeString = value.ToString();
            }
        }

        public DoseResponseModel Clone(List<DoseResponseModelBenchmarkDose> drmBMDs) {
            var doseResponseModel = new DoseResponseModel() {
                IdDoseResponseModel = IdDoseResponseModel,
                BenchmarkResponseType = BenchmarkResponseType,
                BenchmarkResponseTypeString = BenchmarkResponseTypeString,
                Covariates = Covariates, 
                CriticalEffectSize = CriticalEffectSize,
                Description = Description,
                DoseResponseModelBenchmarkDoses = drmBMDs.ToDictionary(r => r.Key),
                DoseUnitString = DoseUnitString,
                IdExperiment = IdExperiment,
                LogLikelihood = LogLikelihood,
                ModelEquation = ModelEquation,
                ModelParameterValues = ModelParameterValues,
                Name = Name,
                Response = Response,
                Substances = Substances,
                DoseResponseModelType = DoseResponseModelType,
                ProastVersion = ProastVersion,
                ExceptionMessage = ExceptionMessage,
            };
            return doseResponseModel;
        }
    }
}
