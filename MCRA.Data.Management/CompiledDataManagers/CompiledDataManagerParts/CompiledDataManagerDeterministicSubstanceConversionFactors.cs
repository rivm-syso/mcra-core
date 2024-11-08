using MCRA.Data.Compiled.Objects;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all deterministic substance conversion factors.
        /// </summary>
        public IList<DeterministicSubstanceConversionFactor> GetAllDeterministicSubstanceConversionFactors() {
            if (_data.AllDeterministicSubstanceConversionFactors == null) {
                LoadScope(SourceTableGroup.DeterministicSubstanceConversionFactors);
                var allConversionFactors = new List<DeterministicSubstanceConversionFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DeterministicSubstanceConversionFactors);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    GetAllFoods();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDeterministicSubstanceConversionFactors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idMeasuredSubstance = r.GetString(RawDeterministicSubstanceConversionFactors.IdMeasuredSubstance, fieldMap);
                                    var idActiveSubstance = r.GetString(RawDeterministicSubstanceConversionFactors.IdActiveSubstance, fieldMap);
                                    var idFood = r.GetStringOrNull(RawDeterministicSubstanceConversionFactors.IdFood, fieldMap);
                                    var noFood = string.IsNullOrEmpty(idFood);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idMeasuredSubstance)
                                              & CheckLinkSelected(ScopingType.Compounds, idActiveSubstance)
                                              & (noFood || CheckLinkSelected(ScopingType.Foods, idFood));
                                    if (valid) {
                                        var food = noFood ? null : _data.GetOrAddFood(idFood);
                                        var record = new DeterministicSubstanceConversionFactor {
                                            MeasuredSubstance = _data.GetOrAddSubstance(idMeasuredSubstance),
                                            ActiveSubstance = _data.GetOrAddSubstance(idActiveSubstance),
                                            ConversionFactor = r.GetDouble(RawDeterministicSubstanceConversionFactors.ConversionFactor, fieldMap),
                                            Reference = r.GetStringOrNull(RawDeterministicSubstanceConversionFactors.Reference, fieldMap),
                                            Food = food,
                                        };
                                        allConversionFactors.Add(record);
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllDeterministicSubstanceConversionFactors = allConversionFactors;
            }
            return _data.AllDeterministicSubstanceConversionFactors;
        }

        private static void writeDeterministicSubstanceConversionFactorsDataToCsv(string tempFolder, IEnumerable<DeterministicSubstanceConversionFactor> types) {
            if (!types?.Any() ?? true) {
                return;
            }

            var tableDef = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.DeterministicSubstanceConversionFactors);
            var dataTable = tableDef.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawDeterministicSubstanceConversionFactors)).Length];
            foreach (var t in types) {
                var r = dataTable.NewRow();
                r.WriteNonEmptyString(RawDeterministicSubstanceConversionFactors.IdActiveSubstance, t.ActiveSubstance?.Code, ccr);
                r.WriteNonEmptyString(RawDeterministicSubstanceConversionFactors.IdMeasuredSubstance, t.MeasuredSubstance?.Code, ccr);
                r.WriteNonNaNDouble(RawDeterministicSubstanceConversionFactors.ConversionFactor, t.ConversionFactor, ccr);
                r.WriteNonEmptyString(RawDeterministicSubstanceConversionFactors.Reference, t.Reference, ccr);
                r.WriteNonEmptyString(RawDeterministicSubstanceConversionFactors.IdFood, t.Food?.Code, ccr);
                dataTable.Rows.Add(r);
            }
            writeToCsv(tempFolder, tableDef, dataTable);
        }
    }
}
