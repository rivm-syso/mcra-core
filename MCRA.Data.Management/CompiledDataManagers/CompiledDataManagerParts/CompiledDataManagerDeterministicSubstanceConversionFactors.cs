using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

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
    }
}
