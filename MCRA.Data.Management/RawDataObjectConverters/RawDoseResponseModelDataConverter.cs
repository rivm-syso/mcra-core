using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawDoseResponseModelDataConverter : RawTableGroupDataConverterBase<RawDoseResponseModelData> {

        public override RawDoseResponseModelData FromCompiledData(CompiledData data) {
            return ToRaw(data.AllDoseResponseModels?.Values);
        }

        public RawDoseResponseModelData ToRaw(IEnumerable<DoseResponseModel> records) {
            if (!records?.Any() ?? true) {
                return null;
            }
            var data = new RawDoseResponseModelData();
            foreach (var model in records) {
                var rawDoseResponseModel = new RawDoseResponseModel() {
                    idDoseResponseModel = model.IdDoseResponseModel,
                    idExperiment = model.IdExperiment,
                    LogLikelihood = model.LogLikelihood,
                    ModelEquation = model.ModelEquation,
                    Name = model.Name,
                    Description = model.Description,
                    DoseResponseModelType = model.DoseResponseModelType,
                    Covariates = model.Covariates != null ? string.Join(", ", model.Covariates) : null,
                    CriticalEffectSize = model.CriticalEffectSize,
                    BenchmarkResponseType = model.BenchmarkResponseType,
                    DoseUnit = model.DoseUnitString,
                    Substances = model.Substances != null ? string.Join(",", model.Substances.Select(r => r.Code)) : null,
                    ProastVersion = model.ProastVersion,
                    idResponse = model.Response?.Code,
                };
                data.DoseResponseModels.Add(rawDoseResponseModel);
                if (model.DoseResponseModelBenchmarkDoses != null) {
                    foreach (var benchmarkDoseRecord in model.DoseResponseModelBenchmarkDoses.Values) {
                        var rawBenchmarkDoseRecord = new RawDoseResponseModelBenchmarkDose() {
                            idDoseResponseModel = model.IdDoseResponseModel,
                            idSubstance = benchmarkDoseRecord.Substance.Code,
                            ModelParameterValues = benchmarkDoseRecord.ModelParameterValues,
                            BenchmarkDose = benchmarkDoseRecord.BenchmarkDose,
                            BenchmarkDoseLower = benchmarkDoseRecord.BenchmarkDoseLower,
                            BenchmarkDoseUpper = benchmarkDoseRecord.BenchmarkDoseUpper,
                            Rpf = benchmarkDoseRecord.Rpf,
                            RpfLower = benchmarkDoseRecord.RpfLower,
                            RpfUpper = benchmarkDoseRecord.RpfUpper,
                            BenchmarkResponse = benchmarkDoseRecord.BenchmarkResponse,
                            Covariates = benchmarkDoseRecord.CovariateLevel,
                        };
                        data.BenchmarkDoses.Add(rawBenchmarkDoseRecord);
                        foreach (var uncertainRecord in benchmarkDoseRecord.DoseResponseModelBenchmarkDoseUncertains) {
                            var rawBenchmarkDoseUncertainRecord = new RawDoseResponseModelBenchmarkDoseUncertain() {
                                idDoseResponseModel = model.IdDoseResponseModel,
                                idSubstance = uncertainRecord.Substance.Code,
                                Covariates = uncertainRecord.CovariateLevel,
                                idUncertaintySet = uncertainRecord.IdUncertaintySet,
                                BenchmarkDose = uncertainRecord.BenchmarkDose,
                                Rpf = uncertainRecord.Rpf,
                            };
                            data.BenchmarkDosesUncertain.Add(rawBenchmarkDoseUncertainRecord);
                        }
                    }
                }
            }
            return data;
        }

        public static DoseResponseModel ToCompiled(
            RawDoseResponseModel r,
            IEnumerable<RawDoseResponseModelBenchmarkDose> benchmarkDoseRecords,
            IEnumerable<RawDoseResponseModelBenchmarkDoseUncertain> benchmarkDoseUncertainRecords,
            Response response,
            ICollection<Compound> allCompounds,
            string doseUnitString
        ) {
            var allSubstances = allCompounds.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);
            var bmduLookup = benchmarkDoseUncertainRecords?
                .Select(record => new DoseResponseModelBenchmarkDoseUncertain() {
                    IdDoseResponseModel = r.idDoseResponseModel,
                    IdUncertaintySet = record.idUncertaintySet,
                    Substance = allSubstances[record.idSubstance],
                    CovariateLevel = record.Covariates,
                    BenchmarkDose = record.BenchmarkDose,
                    Rpf = record.Rpf,
                })
                .ToLookup(rec => rec.Key);

            var result = new DoseResponseModel() {
                IdDoseResponseModel = r.idDoseResponseModel,
                Name = r.Name,
                Description = r.Description,
                DoseResponseModelType = r.DoseResponseModelType,
                Covariates = r.Covariates.Split(',').Select(c => c.Trim()).ToList(),
                CriticalEffectSize = r.CriticalEffectSize,
                BenchmarkResponseType = r.BenchmarkResponseType ?? General.BenchmarkResponseType.Undefined,
                Response = response,
                Substances = r.Substances.Split(',').Select(c => allCompounds.First(s => string.Equals(s.Code, c.Trim(), StringComparison.OrdinalIgnoreCase))).ToList(),
                DoseUnitString = doseUnitString,
                IdExperiment = r.idExperiment,
                LogLikelihood = r.LogLikelihood,
                ModelEquation = r.ModelEquation,
                ProastVersion = r.ProastVersion,
                DoseResponseModelBenchmarkDoses = benchmarkDoseRecords
                    .Where(bmd => bmd.idDoseResponseModel == r.idDoseResponseModel)
                    .Select(bmd => {
                        var drmbmd = new DoseResponseModelBenchmarkDose() {
                            IdDoseResponseModel = r.idDoseResponseModel,
                            Substance = allSubstances[bmd.idSubstance],
                            CovariateLevel = bmd.Covariates,
                            ModelParameterValues = bmd.ModelParameterValues,
                            BenchmarkResponse = bmd.BenchmarkResponse,
                            BenchmarkDose = bmd.BenchmarkDose,
                            Rpf = bmd.Rpf ?? double.NaN,
                            RpfLower = bmd.RpfLower ?? double.NaN,
                            RpfUpper = bmd.RpfUpper ?? double.NaN,
                            BenchmarkDoseLower = bmd.BenchmarkDoseLower ?? double.NaN,
                            BenchmarkDoseUpper = bmd.BenchmarkDoseUpper ?? double.NaN,
                        };
                        drmbmd.DoseResponseModelBenchmarkDoseUncertains = (bmduLookup?.Contains(drmbmd.Key) ?? false) ? bmduLookup[drmbmd.Key].ToList() : new List<DoseResponseModelBenchmarkDoseUncertain>();
                        return drmbmd;
                    })
                    .ToDictionary(drmbd => drmbd.Key),
            };
            return result;
        }
    }
}
