using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// All TDS food sample compositions
        /// </summary>
        public IList<TDSFoodSampleComposition> GetAllTDSFoodSampleCompositions() {
            if (_data.AllTDSFoodSampleCompositions == null) {
                LoadScope(SourceTableGroup.TotalDietStudy);
                _data.AllTDSFoodSampleCompositions = new List<TDSFoodSampleComposition>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.TotalDietStudy);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFoods();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawTDSFoodSampleCompositions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idTdsFood = r.GetString(RawTDSFoodSampleCompositions.IdTdsFood, fieldMap);
                                    var idFood = r.GetString(RawTDSFoodSampleCompositions.IdFood, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Foods, idTdsFood)
                                              & CheckLinkSelected(ScopingType.Foods, idFood);
                                    if (valid) {
                                        var tdsFood = getOrAddFood(idTdsFood);
                                        var food = getOrAddFood(idFood);
                                        var fsc = new TDSFoodSampleComposition {
                                            Food = food,
                                            TDSFood = tdsFood,
                                            PooledAmount = r.GetDouble(RawTDSFoodSampleCompositions.PooledAmount, fieldMap),
                                            Regionality = r.GetStringOrNull(RawTDSFoodSampleCompositions.Regionality, fieldMap),
                                            Seasonality = r.GetStringOrNull(RawTDSFoodSampleCompositions.Seasonality, fieldMap),
                                            Description = r.GetStringOrNull(RawTDSFoodSampleCompositions.Description, fieldMap)
                                        };
                                        _data.AllTDSFoodSampleCompositions.Add(fsc);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return _data.AllTDSFoodSampleCompositions;
        }

        private static void writeFoodSampleCompositionsToCsv(string tempFolder, IEnumerable<TDSFoodSampleComposition> compositions) {
            if (!compositions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.TdsFoodSampleCompositions);
            var dt = td.CreateDataTable();

            foreach (var c in compositions) {
                var row = dt.NewRow();

                row.WriteNonEmptyString(RawTDSFoodSampleCompositions.IdFood, c.Food.Code);
                row.WriteNonEmptyString(RawTDSFoodSampleCompositions.IdTdsFood, c.TDSFood.Code);
                row.WriteNonEmptyString(RawTDSFoodSampleCompositions.Regionality, c.Regionality);
                row.WriteNonEmptyString(RawTDSFoodSampleCompositions.Seasonality, c.Seasonality);
                row.WriteNonEmptyString(RawTDSFoodSampleCompositions.Description, c.Description);
                row.WriteNonNaNDouble(RawTDSFoodSampleCompositions.PooledAmount, c.PooledAmount);

                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt);
        }


        private static void writeConcentrationDistributionsDataToCsv(string tempFolder, IEnumerable<ConcentrationDistribution> distributions) {
            if (!distributions?.Any() ?? true) {
                return;
            }

            var td = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ConcentrationDistributions);
            var dt = td.CreateDataTable();

            foreach (var d in distributions) {
                var row = dt.NewRow();

                row.WriteNonEmptyString(RawConcentrationDistributions.IdFood, d.Food.Code);
                row.WriteNonEmptyString(RawConcentrationDistributions.IdCompound, d.Compound.Code);
                row.WriteNonNaNDouble(RawConcentrationDistributions.Mean, d.Mean);
                row.WriteNonNullDouble(RawConcentrationDistributions.Percentile, d.Percentile);
                row.WriteNonNullDouble(RawConcentrationDistributions.Percentage, d.Percentage);
                row.WriteNonNullDouble(RawConcentrationDistributions.Limit, d.Limit);

                dt.Rows.Add(row);
            }
            writeToCsv(tempFolder, td, dt);
        }
    }
}
