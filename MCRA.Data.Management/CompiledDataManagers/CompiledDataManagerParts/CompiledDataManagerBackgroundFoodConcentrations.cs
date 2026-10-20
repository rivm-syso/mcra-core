using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        /// <summary>
        /// Returns a list of background concentrations.
        /// </summary>
        public ICollection<BackgroundFoodConcentration> GetAllBackgroundFoodConcentrations() {
            if (_data.AllBackgroundFoodConcentrations == null) {
                LoadScope(SourceTableGroup.BackgroundFoodConcentrations);
                var allBackgroundFoodConcentrations = new List<BackgroundFoodConcentration>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.BackgroundFoodConcentrations);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        // Read background food concentrations
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawBackgroundFoodConcentrations>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetStringOrNull(RawBackgroundFoodConcentrations.IdSubstance, fieldMap);
                                    var idFood = r.GetStringOrNull(RawBackgroundFoodConcentrations.IdFood, fieldMap);
                                    var valid = (CheckLinkSelected(ScopingType.Compounds, idSubstance))
                                        & (CheckLinkSelected(ScopingType.Foods, idFood));
                                    if (!valid) {
                                        continue;
                                    }

                                    var food = getOrAddFood(idFood);
                                    var compound = _data.GetOrAddSubstance(idSubstance);
                                    var bgfc = new BackgroundFoodConcentration {
                                        Substance = compound,
                                        Food = food,
                                        Value = r.GetDouble(RawBackgroundFoodConcentrations.Value, fieldMap),
                                        Reference = r.GetStringOrNull(RawBackgroundFoodConcentrations.Reference, fieldMap),
                                        ConcentrationUnit = r.GetEnum(RawBackgroundFoodConcentrations.ConcentrationUnit, fieldMap, ConcentrationUnit.mgPerKg)
                                    };
                                    allBackgroundFoodConcentrations.Add(bgfc);
                                }
                            }
                        }
                    }
                }
                _data.AllBackgroundFoodConcentrations = allBackgroundFoodConcentrations;
            }
            return _data.AllBackgroundFoodConcentrations;
        }
    }
}
