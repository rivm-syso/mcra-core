using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All compounds of the project's compiled data source.
        /// </summary>
        public IDictionary<string, Compound> GetAllCompounds() {
            if (_data.AllSubstances == null) {
                LoadScope(SourceTableGroup.Compounds);
                var allCompounds = new Dictionary<string, Compound>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Compounds);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawCompounds>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var compoundId = r.GetString(RawCompounds.IdCompound, fieldMap);
                                    if (IsCodeSelected(ScopingType.Compounds, compoundId)) {
                                        var substance = new Compound {
                                            Code = compoundId,
                                            Name = r.GetStringOrNull(RawCompounds.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawCompounds.Description, fieldMap),
                                            ConcentrationUnit = r.GetEnum(RawCompounds.ConcentrationUnit, fieldMap, ConcentrationUnit.mgPerKg),
                                            CramerClass = r.GetIntOrNull(RawCompounds.CramerClass, fieldMap),
                                            MolecularMass = r.GetDoubleOrNull(RawCompounds.MolecularMass, fieldMap) ?? double.NaN,
                                            IsLipidSoluble = r.GetBooleanOrNull(RawCompounds.IsLipidSoluble, fieldMap) ?? false,
                                        };
                                        allCompounds[compoundId] = substance;
                                    }
                                }
                            }
                        }

                        // Add items by code from the scope where no matched items were found in the source
                        foreach (var code in GetCodesInScope(ScopingType.Compounds).Except(allCompounds.Keys, StringComparer.OrdinalIgnoreCase)) {
                            allCompounds[code] = new Compound { Code = code };
                        }
                    }
                }
                _data.AllSubstances = allCompounds;
            }
            return _data.AllSubstances;
        }
    }
}
