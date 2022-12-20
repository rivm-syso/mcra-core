using MCRA.Utils.DataFileReading;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Utils;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {
        /// <summary>
        /// Returns a list of default processing factors.
        /// </summary>
        public IList<ProcessingFactor> GetAllProcessingFactors() {
            if (_data.AllProcessingFactors == null) {
                LoadScope(SourceTableGroup.Processing);
                var allProcessingFactors = new List<ProcessingFactor>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.Processing);
                if (rawDataSourceIds?.Any() ?? false) {
                    GetAllFoods();
                    GetAllCompounds();
                    GetAllProcessingTypes();

                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {

                        // Read processing factors
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawProcessingFactors>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetStringOrNull(RawProcessingFactors.IdCompound, fieldMap);
                                    var noSubstance = string.IsNullOrEmpty(idSubstance);
                                    var idProcessedFood = r.GetStringOrNull(RawProcessingFactors.IdFoodProcessed, fieldMap);
                                    var noFoodProcessed = string.IsNullOrEmpty(idProcessedFood);
                                    var idUnProcessedFood = r.GetString(RawProcessingFactors.IdFoodUnprocessed, fieldMap);
                                    var idProcessingType = r.GetString(RawProcessingFactors.IdProcessingType, fieldMap);

                                    var valid = (noSubstance || CheckLinkSelected(ScopingType.Compounds, idSubstance))
                                              & (noFoodProcessed || CheckLinkSelected(ScopingType.Foods, idProcessedFood))
                                              & CheckLinkSelected(ScopingType.Foods, idUnProcessedFood)
                                              & CheckLinkSelected(ScopingType.ProcessingTypes, idProcessingType);

                                    if (!valid) {
                                        continue;
                                    }

                                    var foodProcessed = noFoodProcessed ? null : getOrAddFood(idProcessedFood);
                                    var foodUnprocessed = getOrAddFood(idUnProcessedFood);
                                    var compound = noSubstance ? null : _data.GetOrAddSubstance(idSubstance);
                                    var procType = _data.GetOrAddProcessingType(idProcessingType);

                                    var nominal = r.GetDouble(RawProcessingFactors.Nominal, fieldMap);
                                    var upper = r.GetDoubleOrNull(RawProcessingFactors.Upper, fieldMap);
                                    var nominalUncertaintyUpper = r.GetDoubleOrNull(RawProcessingFactors.NominalUncertaintyUpper, fieldMap);
                                    var upperUncertaintyUpper = r.GetDoubleOrNull(RawProcessingFactors.UpperUncertaintyUpper, fieldMap);
                                    var pf = new ProcessingFactor {
                                        ProcessingType = procType,
                                        Compound = compound,
                                        FoodUnprocessed = foodUnprocessed,
                                        FoodProcessed = foodProcessed,
                                        Nominal = nominal,
                                        Upper = upper,
                                        NominalUncertaintyUpper = nominalUncertaintyUpper,
                                        UpperUncertaintyUpper = upperUncertaintyUpper
                                    };
                                    allProcessingFactors.Add(pf);
                                }
                            }
                        }
                    }
                }
                _data.AllProcessingFactors = allProcessingFactors;
            }
            return _data.AllProcessingFactors;
        }

        private static void writeProcessingTypesDataToCsv(string tempFolder, IEnumerable<ProcessingType> types) {
            if (!types?.Any() ?? true) {
                return;
            }

            var tdProcessingTypes = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ProcessingTypes);
            var dtProcessingTypes = tdProcessingTypes.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawProcessingTypes)).Length];
            foreach(var t in types) {
                var r = dtProcessingTypes.NewRow();
                r.WriteNonEmptyString(RawProcessingTypes.Name, t.Name, ccr);
                r.WriteNonEmptyString(RawProcessingTypes.Description, t.Description, ccr);
                r.WriteNonEmptyString(RawProcessingTypes.IdProcessingType, t.Code, ccr);
                r.WriteNonNullBoolean(RawProcessingTypes.BulkingBlending, t.IsBulkingBlending, ccr);
                r.WriteNonEmptyString(RawProcessingTypes.DistributionType, t.DistributionTypeString, ccr);
                dtProcessingTypes.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdProcessingTypes, dtProcessingTypes);
        }

        private static void writeProcessingFactorsDataToCsv(string tempFolder, IEnumerable<ProcessingFactor> factors) {
            if (!factors?.Any() ?? true) {
                return;
            }

            var tdProcessingFactors = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.ProcessingFactors);
            var dtProcessingFactors = tdProcessingFactors.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawProcessingFactors)).Length];
            foreach (var t in factors) {
                var r = dtProcessingFactors.NewRow();
                r.WriteNonEmptyString(RawProcessingFactors.IdCompound, t.Compound?.Code, ccr);
                r.WriteNonEmptyString(RawProcessingFactors.IdFoodProcessed, t.FoodProcessed.Code, ccr);
                r.WriteNonEmptyString(RawProcessingFactors.IdFoodUnprocessed, t.FoodUnprocessed.Code, ccr);
                r.WriteNonEmptyString(RawProcessingFactors.IdProcessingType, t.ProcessingType.Code, ccr);
                r.WriteNonNaNDouble(RawProcessingFactors.Nominal, t.Nominal, ccr);
                r.WriteNonNullDouble(RawProcessingFactors.NominalUncertaintyUpper, t.NominalUncertaintyUpper, ccr);
                r.WriteNonNullDouble(RawProcessingFactors.Upper, t.Upper, ccr);
                r.WriteNonNullDouble(RawProcessingFactors.UpperUncertaintyUpper, t.UpperUncertaintyUpper, ccr);
                dtProcessingFactors.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdProcessingFactors, dtProcessingFactors);
        }
    }
}
