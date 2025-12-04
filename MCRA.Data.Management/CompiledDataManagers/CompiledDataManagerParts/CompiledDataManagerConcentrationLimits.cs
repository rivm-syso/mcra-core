using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All maximum residue limits.
        /// </summary>
        public IList<ConcentrationLimit> GetAllMaximumConcentrationLimits() {
            if (_data.AllMaximumConcentrationLimits == null) {
                LoadScope(SourceTableGroup.MaximumResidueLimits);
                var allMaximumConcentrationLimits = new List<ConcentrationLimit>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.MaximumResidueLimits);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawMaximumResidueLimits>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawMaximumResidueLimits.IdFood, fieldMap);
                                    var idSubstance = r.GetString(RawMaximumResidueLimits.IdCompound, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                              & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var limit = new ConcentrationLimit {
                                            Food = getOrAddFood(idFood),
                                            Compound = _data.GetOrAddSubstance(idSubstance),
                                            Limit = r.GetDouble(RawMaximumResidueLimits.Limit, fieldMap),
                                            ConcentrationUnit = r.GetEnum(RawMaximumResidueLimits.ConcentrationUnit, fieldMap, ConcentrationUnit.mgPerKg),
                                            ValueType = r.GetEnum(RawMaximumResidueLimits.ValueType, fieldMap, ConcentrationLimitValueType.MaximumResidueLimit),
                                            StartDate = r.GetDateTimeOrNull(RawMaximumResidueLimits.StartDate, fieldMap),
                                            EndDate = r.GetDateTimeOrNull(RawMaximumResidueLimits.EndDate, fieldMap),
                                            Reference = r.GetStringOrNull(RawMaximumResidueLimits.Reference, fieldMap)
                                        };
                                        allMaximumConcentrationLimits.Add(limit);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllMaximumConcentrationLimits = allMaximumConcentrationLimits;
            }
            return _data.AllMaximumConcentrationLimits;
        }
    }
}
