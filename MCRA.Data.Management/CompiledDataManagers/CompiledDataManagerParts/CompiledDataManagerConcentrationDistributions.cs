using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {


        /// <summary>
        /// Return all concentration distributions of the compiled data source.
        /// </summary>
        /// <returns></returns>
        public IList<ConcentrationDistribution> GetAllConcentrationDistributions() {
            if (_data.AllConcentrationDistributions == null) {
                var concentrationDistributions = new List<ConcentrationDistribution>();
                GetAllFoods();
                GetAllCompounds();
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    concentrationDistributions.AddRange(getConcentrationDistributions(rdm));
                }
                _data.AllConcentrationDistributions = concentrationDistributions;
            }
            return _data.AllConcentrationDistributions;
        }

        private List<ConcentrationDistribution> getConcentrationDistributions(IRawDataManager rdm) {
            var concentrationDistributions = new List<ConcentrationDistribution>();
            var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.ConcentrationDistributions);
            if (rawDataSourceIds?.Count > 0) {
                foreach (var rawDataSourceId in rawDataSourceIds) {
                    using (var r = rdm.OpenDataReader<RawConcentrationDistributions>(rawDataSourceId, out int[] fieldMap)) {
                        while (r?.Read() ?? false) {
                            var idFood = r.GetString(RawConcentrationDistributions.IdFood, fieldMap);
                            var idSubstance = r.GetString(RawConcentrationDistributions.IdCompound, fieldMap);
                            var valid = CheckLinkSelected(ScopingType.Foods, idFood)
                                      & CheckLinkSelected(ScopingType.Compounds, idSubstance);
                            if (valid) {
                                var food = getOrAddFood(idFood);
                                var compound = _data.GetOrAddSubstance(idSubstance);
                                var cd = new ConcentrationDistribution {
                                    Compound = compound,
                                    Food = food,
                                    Mean = r.GetDouble(RawConcentrationDistributions.Mean, fieldMap),
                                    CV = r.GetDoubleOrNull(RawConcentrationDistributions.CV, fieldMap),
                                    Percentile = r.GetDoubleOrNull(RawConcentrationDistributions.Percentile, fieldMap),
                                    Percentage = r.GetDoubleOrNull(RawConcentrationDistributions.Percentage, fieldMap),
                                    Limit = r.GetDoubleOrNull(RawConcentrationDistributions.Limit, fieldMap),
                                    ConcentrationUnit = r.GetEnum(RawConcentrationDistributions.ConcentrationUnit, fieldMap, ConcentrationUnit.mgPerKg),
                                };
                                concentrationDistributions.Add(cd);
                            }
                        }
                    }
                }
            }
            return concentrationDistributions;
        }
    }
}
