using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseModelSection : DoseResponseExperimentSection {

        [Display(Name = "Dose response model id", Order = 0)]
        [Description("The id of the dose-response model.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string IdDoseResponseModel { get; set; }

        [Display(Name = "Benchmark response", Order = 8)]
        [Description("The benchmark response.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double CriticalEffectSize { get; set; }

        [Display(Name = "Benchmark response type", Order = 9)]
        [Description("Benchmark response type.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string BenchmarkResponseType { get; set; }

        [Display(Name = "Benchmark dose", Order = 10)]
        [Description("Benchmark dose.")]
        public string BenchmarkDose { get; set; }

        [Display(Name = "Model type", Order = 11)]
        public string ModelType { get; set; }

        [Display(AutoGenerateField = false)]
        [DisplayName("Model equation")]
        public string ModelEquation { get; set; }

        [Display(Name = "Log-likelihood", Order = 12)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? LogLikelihood { get; set; }

        [Display(Name = "Bootstrap runs", Order = 13)]
        public int? NumberOfBootstraps { get; set; }

        [Display(Name = "Converged", Order = 14)]
        public bool Converged { get; set; }

        [Display(Name = "Proast version", Order = 15)]
        [Description("Proast version used to fit the dose-response model.")]
        public string ProastVersion { get; set; }

        [Display(AutoGenerateField = false)]
        public List<DoseResponseFitRecord> DoseResponseFits { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsMultipleSubstanceSingleCovariate {
            get {
                return DoseResponseFits.SelectMany(r => r.CovariateLevel).Distinct().Count() == 1
                    && DoseResponseFits.SelectMany(r => r.SubstanceCode).Distinct().Count() > 1;
            }
        }

        [Display(Name = "Failure message", Order = 14)]
        public string Message { get; set; }

        public void Summarize(
            DoseResponseModel doseResponseModel,
            DoseResponseExperiment experiment,
            Response response,
            Compound referenceCompound,
            ICollection<EffectRepresentation> effectRepresentations
        ) {
            if (experiment != null) {
                Summarize(experiment, response);
            } else {
                ExperimentCode = doseResponseModel.IdExperiment;
                ResponseCode = response.Code;
                ResponseUnit = response.ResponseUnit;
                ResponseType = response.ResponseType;
                var substances = doseResponseModel.Substances.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
                SubstanceCodes = substances.Select(r => r.Code).ToList();
                SubstanceNames = substances.Select(r => r.Name).ToList();
            }

            IdDoseResponseModel = doseResponseModel?.IdDoseResponseModel;
            var hasBenchmarkResponse = effectRepresentations?.Any(r => r.HasBenchmarkResponse()) ?? false;
            CriticalEffectSize = hasBenchmarkResponse ? doseResponseModel?.CriticalEffectSize ?? double.NaN : double.NaN;
            BenchmarkResponseType = doseResponseModel?.BenchmarkResponseType.ToString();
            ModelType = doseResponseModel?.ModelEquation;
            ModelEquation = doseResponseModel?.ModelEquation;
            LogLikelihood = doseResponseModel?.LogLikelihood;
            NumberOfBootstraps = doseResponseModel?.GetNumberOfBootstraps();
            Converged = doseResponseModel?.DoseResponseModelBenchmarkDoses?.Count > 0;
            Message = doseResponseModel?.ExceptionMessage;
            ProastVersion = doseResponseModel?.ProastVersion;
            if (doseResponseModel != null && Converged && hasBenchmarkResponse) {
                var benchmarkDose = doseResponseModel?.DoseResponseModelBenchmarkDoses.Values.Select(c => c.BenchmarkDose.ToString("G3"));
                BenchmarkDose = string.Join("; ", benchmarkDose);
            }

            if (Converged) {
                var referenceBenchmarkDose = doseResponseModel.DoseResponseModelBenchmarkDoses.Values.OrderByDescending(c => c.Substance == referenceCompound ? 1 : 0).FirstOrDefault();

                DoseResponseFits = [];

                var covariateLevels = doseResponseModel.GetCovariateLevels();
                var referenceBmd = (referenceBenchmarkDose != null) ? referenceBenchmarkDose.BenchmarkDose : double.NaN;
                var recordRpf = doseResponseModel.Substances.Count > 1 && covariateLevels.Count <= 1;

                foreach (var substance in doseResponseModel.Substances) {
                    foreach (var level in covariateLevels) {
                        var benchmarkDoseRecord = doseResponseModel.DoseResponseModelBenchmarkDoses.Values
                            .First(c => c.Substance == substance
                                && c.CovariateLevel.Equals(level, StringComparison.InvariantCultureIgnoreCase));

                        var doseResponseSets = DoseResponseSets?.Where(c => c.SubstanceCode.Equals(substance.Code, StringComparison.InvariantCultureIgnoreCase)) ?? new List<DoseResponseSet>();
                        if (!string.IsNullOrEmpty(level)) {
                            doseResponseSets = doseResponseSets.Where(r => r.CovariateLevel.Equals(level, StringComparison.InvariantCultureIgnoreCase));
                        }

                        var rpf = recordRpf ? referenceBmd / benchmarkDoseRecord.BenchmarkDose : 1;
                        foreach (var doseResponseSet in doseResponseSets) {
                            doseResponseSet.RPF = rpf;
                        }
                        if (DoseResponseMixtureSet != null && !DoseResponseMixtureSet.RPFDict.ContainsKey(substance.Code)) {
                            DoseResponseMixtureSet.RPFDict.Add(substance.Code, rpf);
                        }

                        // Split parameter values string and limit parameter values to G3
                        string modelParametersG3Join = null;
                        if (!string.IsNullOrEmpty(benchmarkDoseRecord.ModelParameterValues)) {
                            var modelParametersG3List = benchmarkDoseRecord.ModelParameterValues
                                .Split(',').Select(c => c.Split('='))
                                .Where(r => r.Any())
                                .Select(r => (x: r[0], y: Convert.ToDouble(r[1], CultureInfo.InvariantCulture)));
                            modelParametersG3Join = string.Join(",", modelParametersG3List.Select(r => $"{r.x}={r.y.ToString("G3", CultureInfo.InvariantCulture)}"));
                        }

                        DoseResponseFits.Add(new DoseResponseFitRecord() {
                            SubstanceCode = substance.Code,
                            SubstanceName = substance.Name,
                            CovariateLevel = level,
                            ModelParameterValues = modelParametersG3Join,
                            BenchmarkResponse = hasBenchmarkResponse ? benchmarkDoseRecord.BenchmarkResponse : double.NaN,
                            BenchmarkDose = hasBenchmarkResponse ? benchmarkDoseRecord.BenchmarkDose : double.NaN,
                            BenchmarkDoseLower = hasBenchmarkResponse ? benchmarkDoseRecord.BenchmarkDoseLower : double.NaN,
                            BenchmarkDoseUpper = hasBenchmarkResponse ? benchmarkDoseRecord.BenchmarkDoseUpper : double.NaN,
                            BenchmarkDosesUncertain = hasBenchmarkResponse ? benchmarkDoseRecord.DoseResponseModelBenchmarkDoseUncertains?.Select(r => r.BenchmarkDose).ToList() : null,
                            RelativePotencyFactor = benchmarkDoseRecord.Rpf,
                            RpfLower = benchmarkDoseRecord.RpfLower,
                            RpfUpper = benchmarkDoseRecord.RpfUpper,
                            RpfUncertain = hasBenchmarkResponse ? benchmarkDoseRecord.DoseResponseModelBenchmarkDoseUncertains?.Select(r => r.Rpf).ToList() : null,
                        });
                    }
                }
            }
        }
    }
}
