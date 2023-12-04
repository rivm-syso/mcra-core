using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        public IDictionary<string, List<RelativePotencyFactor>> GetAllRelativePotencyFactors() {
            if (_data.AllRelativePotencyFactors == null) {
                LoadScope(SourceTableGroup.RelativePotencyFactors);
                var relativePotencyFactors = new List<RelativePotencyFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.RelativePotencyFactors);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllCompounds();
                    GetAllEffects();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read RPFs
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawRelativePotencyFactors>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idEffect = r.GetString(RawRelativePotencyFactors.IdEffect, fieldMap);
                                        var idSubstance = r.GetString(RawRelativePotencyFactors.IdCompound, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.Effects, idEffect)
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                        if (valid) {
                                            var rpf = new RelativePotencyFactor {
                                                Compound = _data.GetOrAddSubstance(idSubstance),
                                                Effect = _data.GetOrAddEffect(idEffect),
                                                RPF = r.GetDouble(RawRelativePotencyFactors.RPF, fieldMap),
                                                PublicationAuthors = r.GetStringOrNull(RawRelativePotencyFactors.PublicationAuthors, fieldMap),
                                                PublicationTitle = r.GetStringOrNull(RawRelativePotencyFactors.PublicationTitle, fieldMap),
                                                PublicationUri = r.GetStringOrNull(RawRelativePotencyFactors.PublicationUri, fieldMap),
                                                PublicationYear = r.GetIntOrNull(RawRelativePotencyFactors.PublicationYear, fieldMap),
                                                Description = r.GetStringOrNull(RawRelativePotencyFactors.Description, fieldMap),
                                            };
                                            relativePotencyFactors.Add(rpf);
                                        }
                                    }
                                }
                            }
                        }
                        var lookup = relativePotencyFactors.ToDictionary(r => (r.Effect, r.Compound));

                        // Read RPF uncertainties
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawRelativePotencyFactorsUncertain>(rawDataSourceId, out int[] fieldMap)) {
                                if (r != null) {
                                    while (r?.Read() ?? false) {
                                        var idEffect = r.GetString(RawRelativePotencyFactorsUncertain.IdEffect, fieldMap);
                                        var idSubstance = r.GetString(RawRelativePotencyFactorsUncertain.IdCompound, fieldMap);
                                        var valid = CheckLinkSelected(ScopingType.Effects, idEffect)
                                                  & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                        if (valid) {
                                            var substance = _data.GetOrAddSubstance(idSubstance);
                                            var effect = _data.GetOrAddEffect(idEffect);
                                            if (lookup.TryGetValue((effect, substance), out RelativePotencyFactor factor)) {
                                                var rpfu = new RelativePotencyFactorUncertain {
                                                    RPF = r.GetDouble(RawRelativePotencyFactorsUncertain.RPF, fieldMap),
                                                    idUncertaintySet = r.GetString(RawRelativePotencyFactorsUncertain.IdUncertaintySet, fieldMap)
                                                };
                                                factor.RelativePotencyFactorsUncertains.Add(rpfu);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllRelativePotencyFactors = relativePotencyFactors
                    .GroupBy(r => r.Effect.Code)
                    .ToDictionary(r => r.Key, r => r.ToList(), StringComparer.OrdinalIgnoreCase);
            }
            return _data.AllRelativePotencyFactors;
        }

        private static void writeRelativePotencyFactorsDataToCsv(string tempFolder, IEnumerable<RelativePotencyFactor> data) {
            if (!data?.Any() ?? true) {
                return;
            }

            var mapper = new RawRelativePotencyFactorDataConverter();
            var rawData = mapper.ToRaw(data);
            var writer = new CsvRawDataWriter(tempFolder);
            writer.Set(rawData);
            writer.Store();
        }
    }
}
