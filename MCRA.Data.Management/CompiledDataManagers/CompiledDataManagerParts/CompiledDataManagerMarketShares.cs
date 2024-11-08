using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Read all foods from the compiled data source.
        /// </summary>
        /// <returns></returns>
        public IList<MarketShare> GetAllMarketShares() {
            if (_data.AllMarketShares == null) {
                LoadScope(SourceTableGroup.MarketShares);
                var allMarketShares = new List<MarketShare>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.MarketShares);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllFoods();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawMarketShares>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idFood = r.GetString(RawMarketShares.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var food = getOrAddFood(idFood, idFood);
                                        var percentage = r.GetDoubleOrNull(RawMarketShares.Percentage, fieldMap);
                                        var brandLoyalty = r.GetDoubleOrNull(RawMarketShares.BrandLoyalty, fieldMap);
                                        food.MarketShare = new MarketShare {
                                            Food = food,
                                            Percentage = percentage ?? 0D,
                                            BrandLoyalty = brandLoyalty ?? 0D,
                                        };
                                    }
                                }
                            }
                        }
                    }
                }

                _data.AllMarketShares = allMarketShares;
            }
            _data.AllMarketShares = _data.AllFoods.Where(c => c.Value.MarketShare != null).Select(c => c.Value.MarketShare).ToList();
            return _data.AllMarketShares;
        }

        private static void writeMarketSharesDataToCsv(string tempFolder, IList<MarketShare> marketShares) {
            if (!marketShares?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.MarketShares);
            var dt = td.CreateDataTable();
            foreach (var t in marketShares) {
                var row = dt.NewRow();
                row.WriteNonEmptyString(RawMarketShares.IdFood, t.Food.Code);
                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt);
        }
    }
}
