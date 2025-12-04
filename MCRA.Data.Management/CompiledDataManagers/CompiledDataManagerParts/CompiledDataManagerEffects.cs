using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All compounds of the project's compiled data source.
        /// </summary>
        public IDictionary<string, Effect> GetAllEffects() {
            if (_data.AllEffects == null) {
                LoadScope(SourceTableGroup.Effects);
                var allEffects = new Dictionary<string, Effect>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Effects);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawEffects>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idEffect = r.GetString(RawEffects.IdEffect, fieldMap);
                                    if (!IsCodeSelected(ScopingType.Effects, idEffect)) {
                                        continue;
                                    }

                                    var effect = new Effect {
                                        Code = idEffect,
                                        Name = r.GetStringOrNull(RawEffects.Name, fieldMap),
                                        Description = r.GetStringOrNull(RawEffects.Description, fieldMap),
                                        IsGenotoxic = r.GetBooleanOrNull(RawEffects.IsGenotoxic, fieldMap),
                                        IsAChEInhibitor = r.GetBooleanOrNull(RawEffects.IsAChEInhibitor, fieldMap),
                                        IsNonGenotoxicCarcinogenic = r.GetBooleanOrNull(RawEffects.IsNonGenotoxicCarcinogenic, fieldMap),
                                        BiologicalOrganisationType = r.GetEnum<BiologicalOrganisationType>(RawEffects.BiologicalOrganisation, fieldMap),
                                        KeyEventProcess = r.GetStringOrNull(RawEffects.KeyEventProcess, fieldMap),
                                        KeyEventObject = r.GetStringOrNull(RawEffects.KeyEventObject, fieldMap),
                                        KeyEventAction = r.GetStringOrNull(RawEffects.KeyEventAction, fieldMap),
                                        KeyEventCell = r.GetStringOrNull(RawEffects.KeyEventCell, fieldMap),
                                        KeyEventOrgan = r.GetStringOrNull(RawEffects.KeyEventOrgan, fieldMap),
                                        AOPWikiIds = r.GetStringOrNull(RawEffects.AOPWikiIds, fieldMap),
                                        Reference = r.GetStringOrNull(RawEffects.Reference, fieldMap),
                                    };
                                    allEffects[idEffect] = effect;
                                }
                            }
                        }
                    }

                    // Add effects by code from the scope where no matched effects were found in the source
                    foreach (var code in GetCodesInScope(ScopingType.Effects).Except(allEffects.Keys, StringComparer.OrdinalIgnoreCase)) {
                        allEffects[code] = new Effect { Code = code };
                    }
                }
                _data.AllEffects = allEffects;
            }
            return _data.AllEffects;
        }
    }
}
