using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

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
                if (rawDataSourceIds?.Count > 0) {
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
    }
}
