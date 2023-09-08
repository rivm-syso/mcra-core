using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

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
                if (rawDataSourceIds?.Any() ?? false) {
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
                                            ConcentrationUnitString = r.GetStringOrNull(RawCompounds.ConcentrationUnit, fieldMap),
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

        private static void writeCompoundsDataToCsv(string tempFolder, IEnumerable<Compound> compounds) {
            if (!compounds?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.Compounds);
            var dt = td.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawCompounds)).Length];

            foreach (var cmp in compounds) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawCompounds.IdCompound, cmp.Code, ccr);
                row.WriteNonEmptyString(RawCompounds.Name, cmp.Name, ccr);
                row.WriteNonEmptyString(RawCompounds.Description, cmp.Description, ccr);
                row.WriteNonEmptyString(RawCompounds.ConcentrationUnit, cmp.ConcentrationUnitString, ccr);
                row.WriteNonNullInt32(RawCompounds.CramerClass, cmp.CramerClass, ccr);
                row.WriteNonNaNDouble(RawCompounds.MolecularMass, cmp.MolecularMass, ccr);
                row.WriteNonNullBoolean(RawCompounds.IsLipidSoluble, cmp.IsLipidSoluble, ccr);
                dt.Rows.Add(row);
            }

            writeToCsv(tempFolder, td, dt, ccr);
        }
    }
}
